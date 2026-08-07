using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Barliesque.InspectorTools.Editor
{
	public class ReplaceSelectionWindow : EditorWindow
	{
		private GameObject _replacementObject;
		private bool _keepOriginals;

		//[MenuItem("GameObject/Replace Selection...", false, 100)]
		static private void ShowWindow()
		{
			var window = GetWindow<ReplaceSelectionWindow>("Replace Selection");
			window.minSize = new Vector2(300, 150);
		}

		private void OnGUI()
		{
			EditorGUILayout.Space();
			_replacementObject = (GameObject)EditorGUILayout.ObjectField("Replacement Object", _replacementObject, typeof(GameObject), false);
			_keepOriginals = EditorGUILayout.Toggle("Keep Originals", _keepOriginals);

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

			var translationMap = new Dictionary<Object, Object>();

			// 1. Instantiate replacements and map references
			foreach (var targetTransform in selectedTransforms)
			{
				var oldGo = targetTransform.gameObject;
				var assetType = PrefabUtility.GetPrefabAssetType(_replacementObject);
				GameObject newGo;

				if (assetType is PrefabAssetType.Regular or PrefabAssetType.Model)
				{
					newGo = (GameObject)PrefabUtility.InstantiatePrefab(_replacementObject);
				}
				else
				{
					newGo = Instantiate(_replacementObject);
				}

				if (!newGo) continue;

				Undo.RegisterCreatedObjectUndo(newGo, "Instantiate Replacement");

				// Match Transform
				var newTransform = newGo.transform;
				newTransform.parent = targetTransform.parent;
				newTransform.localPosition = targetTransform.localPosition;
				newTransform.localRotation = targetTransform.localRotation;
				newTransform.localScale = targetTransform.localScale;
				newGo.name = _replacementObject.name;

				// Map the GameObject itself
				translationMap[oldGo] = newGo;

				// Map all components from old to new where types match
				var oldComponents = oldGo.GetComponents<Component>();
				var newComponents = newGo.GetComponents<Component>();

				foreach (var oldComp in oldComponents)
				{
					if (!oldComp) continue;
					var type = oldComp.GetType();

					// Find a component of the same type on the new object
					foreach (var newComp in newComponents)
					{
						if (newComp && newComp.GetType() == type)
						{
							translationMap[oldComp] = newComp;
							break;
						}
					}
				}
			}

			// 2. Update references across the scene
			UpdateSceneReferences(translationMap);

			// 3. Cleanup originals
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

		private void UpdateSceneReferences(Dictionary<Object, Object> translationMap)
		{
			if (translationMap.Count == 0) return;

			// Find every component in the scene, including inactive ones
			var allComponents = FindObjectsByType<Component>(FindObjectsInactive.Include, FindObjectsSortMode.None);
			var progress = 0f;
			var total = (float)allComponents.Length;

			for (var i = 0; i < allComponents.Length; i++)
			{
				var comp = allComponents[i];
				if (!comp) continue;

				// Skip components that are part of the replacement objects we just created
				if (translationMap.ContainsValue(comp) || translationMap.ContainsValue(comp.gameObject)) continue;

				EditorUtility.DisplayProgressBar("Replacing References", $"Scanning {comp.gameObject.name}...", i / total);

				var so = new SerializedObject(comp);
				var sp = so.GetIterator();
				var changed = false;

				// Iterate through all serialized properties (fields) of the component
				while (sp.NextVisible(true))
				{
					if (sp.propertyType == SerializedPropertyType.ObjectReference)
					{
						if (sp.objectReferenceValue && translationMap.TryGetValue(sp.objectReferenceValue, out var replacement))
						{
							sp.objectReferenceValue = replacement;
							changed = true;
						}
					}
				}

				if (changed)
				{
					so.ApplyModifiedProperties();
				}
			}

			EditorUtility.ClearProgressBar();
		}
	}
}