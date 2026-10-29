using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Barliesque.InspectorTools.Editor
{
	public class ReplaceSelectionWindow : EditorWindow
	{
		private GameObject _replacementObject;
		private bool _keepOriginals;
		private bool _redirectIncoming = true;
		private bool _transferOutgoing;

		static private readonly GUIContent RedirectIncomingLabel = new GUIContent(
			"Redirect Incoming References",
			"Scene components that referenced the originals (or their children and components) will point to the matching parts of the replacements.");

		static private readonly GUIContent TransferOutgoingLabel = new GUIContent(
			"Transfer Outgoing References",
			"References from the originals to other scene objects are copied onto the matching fields of the replacements. " +
			"References to assets are left as the prefab defines them, except in arrays that must be resized to fit.");

		//[MenuItem("GameObject/Replace Selection...", false, 100)]
		static private void ShowWindow()
		{
			var window = GetWindow<ReplaceSelectionWindow>("Replace Selection");
			window.minSize = new Vector2(300, 190);
		}

		private void OnGUI()
		{
			EditorGUILayout.Space();
			_replacementObject = (GameObject)EditorGUILayout.ObjectField("Replacement Object", _replacementObject, typeof(GameObject), false);
			_keepOriginals = EditorGUILayout.Toggle("Keep Originals", _keepOriginals);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("References", EditorStyles.boldLabel);
			_redirectIncoming = EditorGUILayout.ToggleLeft(RedirectIncomingLabel, _redirectIncoming);
			_transferOutgoing = EditorGUILayout.ToggleLeft(TransferOutgoingLabel, _transferOutgoing);

			EditorGUILayout.Space();

			GUI.enabled = _replacementObject;
			if (GUILayout.Button("Replace Selection", GUILayout.Height(30)))
			{
				PerformReplacement();
			}

			GUI.enabled = true;

			if (!_replacementObject)
			{
				EditorGUILayout.HelpBox("Assign a prefab or object to enable replacement.", MessageType.Info);
			}
		}

		private void PerformReplacement()
		{
			if (!_replacementObject) return;

			var selectedTransforms = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.Editable);
			if (selectedTransforms.Length == 0) return;

			Undo.SetCurrentGroupName($"Replace {selectedTransforms.Length} Objects");
			var group = Undo.GetCurrentGroup();

			// Maps original objects/components to their counterparts in the replacements
			var translationMap = new Dictionary<Object, Object>();
			var componentPairs = new List<(Component oldComp, Component newComp)>();

			// Every GameObject in the original and replacement hierarchies
			var originalObjects = new HashSet<GameObject>();
			var replacementObjects = new HashSet<GameObject>();

			// Covers regular prefabs, variants and models alike
			var isPrefabAsset = PrefabUtility.IsPartOfPrefabAsset(_replacementObject);

			// 1. Instantiate replacements and map references
			foreach (var targetTransform in selectedTransforms)
			{
				var oldGo = targetTransform.gameObject;
				GameObject newGo;

				if (isPrefabAsset)
				{
					// Instantiate into the same scene as the original, keeping the prefab connection
					newGo = (GameObject)PrefabUtility.InstantiatePrefab(_replacementObject, oldGo.scene);
				}
				else
				{
					newGo = Instantiate(_replacementObject);
				}

				if (!newGo) continue;

				Undo.RegisterCreatedObjectUndo(newGo, "Instantiate Replacement");

				// Match hierarchy placement
				var newTransform = newGo.transform;
				Undo.SetTransformParent(newTransform, targetTransform.parent, "Parent Replacement");
				newTransform.SetSiblingIndex(targetTransform.GetSiblingIndex());

				// Match Transform
				newTransform.localPosition = targetTransform.localPosition;
				newTransform.localRotation = targetTransform.localRotation;
				newTransform.localScale = targetTransform.localScale;
				newGo.name = _replacementObject.name;

				var unmatched = new List<string>();
				MapHierarchy(targetTransform, newTransform, translationMap, componentPairs, unmatched);

				if (unmatched.Count > 0)
				{
					Log($"Replacing '{oldGo.name}' with '{newGo.name}': no matching child in the replacement for " +
						$"{string.Join(", ", unmatched)}. References to these (or their descendants) can't be carried over.", newGo);
				}

				CollectHierarchy(oldGo, originalObjects);
				CollectHierarchy(newGo, replacementObjects);
			}

			try
			{
				// 2. Copy the originals' references to other scene objects onto the replacements
				if (_transferOutgoing)
				{
					TransferOutgoingReferences(componentPairs, translationMap, originalObjects);
				}

				// 3. Point other scene components at the replacements
				if (_redirectIncoming)
				{
					RedirectIncomingReferences(translationMap, originalObjects, replacementObjects, _keepOriginals);
				}
			}
			finally
			{
				EditorUtility.ClearProgressBar();
			}

			// 4. Cleanup originals
			if (!_keepOriginals)
			{
				foreach (var targetTransform in selectedTransforms)
				{
					if (targetTransform)
					{
						Undo.DestroyObjectImmediate(targetTransform.gameObject);
					}
				}
			}

			Undo.CollapseUndoOperations(group);
		}

		#region Hierarchy Matching

		/// <summary>
		/// Maps the roots to each other, then walks down both hierarchies matching children.
		/// Paths of original children with no counterpart are added to <paramref name="unmatched"/>.
		/// </summary>
		static private void MapHierarchy(Transform oldRoot, Transform newRoot,
			Dictionary<Object, Object> map, List<(Component, Component)> componentPairs, List<string> unmatched)
		{
			map[oldRoot.gameObject] = newRoot.gameObject;
			MapComponents(oldRoot.gameObject, newRoot.gameObject, map, componentPairs);
			MapChildren(oldRoot, newRoot, oldRoot, map, componentPairs, unmatched);
		}

		static private void MapChildren(Transform oldParent, Transform newParent, Transform oldRoot,
			Dictionary<Object, Object> map, List<(Component, Component)> componentPairs, List<string> unmatched)
		{
			var claimed = new HashSet<Transform>();

			for (var i = 0; i < oldParent.childCount; i++)
			{
				var oldChild = oldParent.GetChild(i);
				var newChild = FindCounterpart(oldChild, newParent, claimed);

				if (!newChild)
				{
					unmatched.Add(GetDisplayPath(oldChild, oldRoot));
					continue;
				}

				claimed.Add(newChild);
				map[oldChild.gameObject] = newChild.gameObject;
				MapComponents(oldChild.gameObject, newChild.gameObject, map, componentPairs);
				MapChildren(oldChild, newChild, oldRoot, map, componentPairs, unmatched);
			}
		}

		/// <summary>
		/// Finds the child of <paramref name="newParent"/> that corresponds to <paramref name="oldChild"/>.
		/// Children are matched by name. Where several siblings share that name, the one at the same
		/// child index is preferred, then the one at the same position among its same-named siblings.
		/// </summary>
		static private Transform FindCounterpart(Transform oldChild, Transform newParent, HashSet<Transform> claimed)
		{
			var candidates = new List<Transform>();
			for (var i = 0; i < newParent.childCount; i++)
			{
				var child = newParent.GetChild(i);
				if (child.name == oldChild.name)
				{
					candidates.Add(child);
				}
			}

			if (candidates.Count == 0) return null;

			// Same name at the same child index
			var index = oldChild.GetSiblingIndex();
			foreach (var candidate in candidates)
			{
				if (candidate.GetSiblingIndex() == index && !claimed.Contains(candidate)) return candidate;
			}

			// Same position among same-named siblings
			var ordinal = GetNameOrdinal(oldChild);
			if (ordinal < candidates.Count && !claimed.Contains(candidates[ordinal])) return candidates[ordinal];

			// Otherwise the first one still available
			foreach (var candidate in candidates)
			{
				if (!claimed.Contains(candidate)) return candidate;
			}

			return null;
		}

		/// <summary>
		/// How many earlier siblings share this transform's name.
		/// </summary>
		static private int GetNameOrdinal(Transform t)
		{
			var parent = t.parent;
			if (!parent) return 0;

			var ordinal = 0;
			for (var i = 0; i < t.GetSiblingIndex(); i++)
			{
				if (parent.GetChild(i).name == t.name) ordinal++;
			}
			return ordinal;
		}

		/// <summary>
		/// A path relative to <paramref name="root"/>, with [n] added to names shared by several siblings.
		/// </summary>
		static private string GetDisplayPath(Transform t, Transform root)
		{
			var path = GetPathSegment(t);
			for (var p = t.parent; p && p != root; p = p.parent)
			{
				path = GetPathSegment(p) + "/" + path;
			}
			return path;
		}

		static private string GetPathSegment(Transform t)
		{
			var parent = t.parent;
			if (!parent) return t.name;

			var sameNameCount = 0;
			for (var i = 0; i < parent.childCount; i++)
			{
				if (parent.GetChild(i).name == t.name) sameNameCount++;
			}

			return sameNameCount > 1 ? $"{t.name}[{GetNameOrdinal(t)}]" : t.name;
		}

		/// <summary>
		/// The nth component of a given type on the old object maps to the nth of that type on the new one.
		/// </summary>
		static private void MapComponents(GameObject oldGo, GameObject newGo,
			Dictionary<Object, Object> map, List<(Component, Component)> componentPairs)
		{
			var newByType = new Dictionary<System.Type, Queue<Component>>();
			foreach (var comp in newGo.GetComponents<Component>())
			{
				if (!comp) continue;
				var type = comp.GetType();
				if (!newByType.TryGetValue(type, out var queue))
				{
					newByType[type] = queue = new Queue<Component>();
				}
				queue.Enqueue(comp);
			}

			foreach (var oldComp in oldGo.GetComponents<Component>())
			{
				if (!oldComp) continue;
				if (newByType.TryGetValue(oldComp.GetType(), out var queue) && queue.Count > 0)
				{
					var newComp = queue.Dequeue();
					map[oldComp] = newComp;
					componentPairs.Add((oldComp, newComp));
				}
			}
		}

		static private void CollectHierarchy(GameObject root, HashSet<GameObject> set)
		{
			foreach (var t in root.GetComponentsInChildren<Transform>(true))
			{
				set.Add(t.gameObject);
			}
		}

		#endregion

		#region Reference Updating

		/// <summary>
		/// For each matched component pair, copies object references that point to scene objects
		/// from the original onto the same field of the replacement. References into the original's
		/// own hierarchy are translated to the replacement's equivalent, if one was matched.
		/// Arrays holding scene references are resized to match the original's when necessary,
		/// in which case the original's whole array (asset references included) is copied first.
		/// </summary>
		static private void TransferOutgoingReferences(List<(Component oldComp, Component newComp)> componentPairs,
			Dictionary<Object, Object> map, HashSet<GameObject> originalObjects)
		{
			var total = (float)componentPairs.Count;

			for (var i = 0; i < componentPairs.Count; i++)
			{
				var (oldComp, newComp) = componentPairs[i];
				if (!oldComp || !newComp || oldComp is Transform) continue;

				EditorUtility.DisplayProgressBar("Transferring References", $"Copying from {oldComp.gameObject.name}...", i / total);

				var oldSo = new SerializedObject(oldComp);
				var newSo = new SerializedObject(newComp);
				var sp = oldSo.GetIterator();
				var enterChildren = true;
				var changed = false;

				while (sp.NextVisible(enterChildren))
				{
					enterChildren = true;

					if (IsArray(sp))
					{
						var targetArray = newSo.FindProperty(sp.propertyPath);
						if (targetArray != null && IsArray(targetArray)
							&& targetArray.arraySize != sp.arraySize
							&& ContainsSceneReference(sp))
						{
							// Sizes differ, so start from a copy of the original's array, then fix up its links
							newSo.CopyFromSerializedProperty(sp);
							TranslateCopiedReferences(newSo.FindProperty(sp.propertyPath), map, originalObjects, newComp);
							changed = true;

							// The whole array is handled, so skip its elements
							enterChildren = false;
						}

						// Same size (or no scene references): elements are handled individually below
						continue;
					}

					if (sp.propertyType != SerializedPropertyType.ObjectReference) continue;

					var value = sp.objectReferenceValue;

					// Leave asset references (scripts, materials, etc.) as the prefab defines them
					if (!value || EditorUtility.IsPersistent(value)) continue;

					if (map.TryGetValue(value, out var translated))
					{
						value = translated;
					}
					else if (IsInHierarchies(value, originalObjects))
					{
						Log($"{Describe(oldComp, sp)} references '{value.name}', which has no counterpart in the replacement, " +
							"so it was not transferred.", newComp);
						continue;
					}

					var target = newSo.FindProperty(sp.propertyPath);
					if (target == null || target.propertyType != SerializedPropertyType.ObjectReference) continue;
					if (target.objectReferenceValue == value) continue;

					target.objectReferenceValue = value;
					changed = true;
				}

				if (changed)
				{
					newSo.ApplyModifiedProperties();
				}
			}
		}

		/// <summary>
		/// Within a freshly copied property, points references to the originals at their replacements.
		/// References into the originals with no counterpart are cleared.
		/// </summary>
		static private void TranslateCopiedReferences(SerializedProperty property,
			Dictionary<Object, Object> map, HashSet<GameObject> originalObjects, Component owner)
		{
			if (property == null) return;

			var it = property.Copy();
			var end = property.GetEndProperty();

			while (it.NextVisible(true) && !SerializedProperty.EqualContents(it, end))
			{
				if (it.propertyType != SerializedPropertyType.ObjectReference) continue;

				var value = it.objectReferenceValue;
				if (!value || EditorUtility.IsPersistent(value)) continue;

				if (map.TryGetValue(value, out var translated))
				{
					it.objectReferenceValue = translated;
				}
				else if (IsInHierarchies(value, originalObjects))
				{
					Log($"{Describe(owner, it)} referenced '{value.name}', which has no counterpart in the replacement, " +
						"so it was cleared.", owner);
					it.objectReferenceValue = null;
				}
			}
		}

		/// <summary>
		/// Points references held by other scene components at the replacements.
		/// </summary>
		static private void RedirectIncomingReferences(Dictionary<Object, Object> map,
			HashSet<GameObject> originalObjects, HashSet<GameObject> replacementObjects, bool keepOriginals)
		{
			if (map.Count == 0) return;

			// Find every component in the scene, including inactive ones
			var allComponents = FindObjectsByType<Component>(FindObjectsInactive.Include, FindObjectsSortMode.None);
			var total = (float)allComponents.Length;

			for (var i = 0; i < allComponents.Length; i++)
			{
				var comp = allComponents[i];
				if (!comp) continue;

				// Originals are handled by the outgoing transfer (or destroyed); replacements are already correct
				var go = comp.gameObject;
				if (originalObjects.Contains(go) || replacementObjects.Contains(go)) continue;

				EditorUtility.DisplayProgressBar("Redirecting References", $"Scanning {go.name}...", i / total);

				var so = new SerializedObject(comp);
				var sp = so.GetIterator();
				var changed = false;

				// Iterate through all serialized properties (fields) of the component
				while (sp.NextVisible(true))
				{
					if (sp.propertyType != SerializedPropertyType.ObjectReference) continue;

					var value = sp.objectReferenceValue;
					if (!value) continue;

					if (map.TryGetValue(value, out var replacement))
					{
						sp.objectReferenceValue = replacement;
						changed = true;
					}
					else if (!keepOriginals && IsInHierarchies(value, originalObjects))
					{
						Log($"{Describe(comp, sp)} references '{value.name}', which has no counterpart in the replacement. " +
							"It will be missing once the original is removed.", comp);
					}
				}

				if (changed)
				{
					so.ApplyModifiedProperties();
				}
			}
		}

		/// <summary>
		/// A generic array or list (strings also report as arrays, so they're excluded).
		/// </summary>
		static private bool IsArray(SerializedProperty property)
		{
			return property.isArray && property.propertyType == SerializedPropertyType.Generic;
		}

		static private bool ContainsSceneReference(SerializedProperty property)
		{
			var it = property.Copy();
			var end = property.GetEndProperty();

			while (it.NextVisible(true) && !SerializedProperty.EqualContents(it, end))
			{
				if (it.propertyType != SerializedPropertyType.ObjectReference) continue;

				var value = it.objectReferenceValue;
				if (value && !EditorUtility.IsPersistent(value)) return true;
			}

			return false;
		}

		static private bool IsInHierarchies(Object obj, HashSet<GameObject> gameObjects)
		{
			var go = obj switch
			{
				GameObject g => g,
				Component c => c.gameObject,
				_ => null
			};
			return go && gameObjects.Contains(go);
		}

		static private string Describe(Component comp, SerializedProperty property)
		{
			return $"'{comp.gameObject.name}' ({comp.GetType().Name}).{property.propertyPath}";
		}

		static private void Log(string message, Object context = null)
		{
			Debug.Log($"<color=yellow>Replace Selection: {message}</color>", context);
		}

		#endregion
	}
}