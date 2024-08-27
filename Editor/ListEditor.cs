using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

// More info:
// https://va.lent.in/unity-make-your-lists-functional-with-reorderablelist/

namespace Barliesque.InspectorTools.Editor
{
	public class ListEditor
	{
		public ReorderableList List { get; }
		
		private bool _foldable;
		public bool ShowIndex = false;

		public string Header
		{
			get => _showHeader ? _header : null;
			set => _header = value;
		}

		public bool IsFolded { get; set; }
		public bool CanAddOrRemove = true;

		private Vector2 _scrollPos;
		
		private string _header;
		private readonly bool _showHeader;
		private float _linesPerElement;

		public Action<SerializedProperty> OnElementAdded;
		

		/// <summary>
		/// Instantiate this from the OnEnable part of your custom Editor.  Then call DoLayoutList() from the OnInspectorGUI method to draw the inspector.
		/// </summary>
		/// <param name="serializedObject">The serialized object containing the property.</param>
		/// <param name="field">The path to the property.</param>
		/// <param name="isFolded">Should the list be folded up by default?</param>
		/// <param name="header">By default, the property name is used as the header.  Assign a custom header here, or an empty string for no header.</param>
		/// <param name="linesPerElement">How many lines per element?</param>
		public ListEditor(SerializedObject serializedObject, string field, bool isFolded, string header = null, float linesPerElement = 1) : this(
			serializedObject.FindProperty(field), header, linesPerElement)
		{
			_foldable = true;
			IsFolded = isFolded;
		}

		/// <summary>
		/// Instantiate this from the OnEnable part of your custom Editor.  Then call DoLayoutList() from the OnInspectorGUI method to draw the inspector.
		/// </summary>
		/// <param name="serializedObject">The serialized object containing the property.</param>
		/// <param name="field">The path to the property.</param>
		/// <param name="header">By default, the property name is used as the header.  Assign a custom header here, or an empty string for no header.</param>
		/// <param name="linesPerElement">How many lines per element?</param>
		public ListEditor(SerializedObject serializedObject, string field, string header = null, float linesPerElement = 1) : this(
			serializedObject.FindProperty(field), header, linesPerElement)
		{ }

		/// <summary>
		/// Instantiate this from the OnEnable part of your custom Editor.  Then call DoLayoutList() from the OnInspectorGUI method to draw the inspector.
		/// </summary>
		/// <param name="property">The serialized property containing the List.</param>
		/// <param name="isFolded">Should the list be folded up by default?</param>
		/// <param name="header">By default, the property name is used as the header.  Assign a custom header here, or an empty string for no header.</param>
		/// <param name="linesPerElement">How many lines per element?</param>
		public ListEditor(SerializedProperty property, bool isFolded, string header = null, float linesPerElement = 1) : this(
			property, header, linesPerElement)
		{
			_foldable = true;
			IsFolded = isFolded;
		}

		/// <summary>
		/// Instantiate this from the OnEnable part of your custom Editor.  Then call DoLayoutList() from the OnInspectorGUI method to draw the inspector.
		/// </summary>
		/// <param name="property">The serialized property containing the List.</param>
		/// <param name="header">By default, the property name is used as the header.  Assign a custom header here, or an empty string for no header.</param>
		/// <param name="linesPerElement">How many lines per element?</param>
		public ListEditor(SerializedProperty property, string header = null, float linesPerElement = 1)
		{
			List = new ReorderableList(property.serializedObject, property, true, !string.IsNullOrEmpty(header), true, true)
			{
				elementHeight = (EditorGUIUtility.singleLineHeight + 4f) * linesPerElement + 5f,
				drawElementCallback = DrawElement,
				drawHeaderCallback = DrawHeader,
				onChangedCallback = ApplyChanges,
				onReorderCallback = ApplyChanges,
				onRemoveCallback = RemoveElement,
				onAddCallback = ElementAdded
			};
			_showHeader = !string.IsNullOrEmpty(header);
			_header = header ?? List.serializedProperty.displayName;
			_linesPerElement = linesPerElement;
			
			//ReorderableList.defaultBehaviours.draggingHandle.margin
		}
		

		private void ElementAdded(ReorderableList list)
		{
			if (!CanAddOrRemove) return;
			// Add a new element
			ReorderableList.defaultBehaviours.DoAddButton(list);
			var count = list.serializedProperty.arraySize;
			var element = list.serializedProperty.GetArrayElementAtIndex(count - 1);
			OnElementAdded?.Invoke(element);
		}
		
		private void RemoveElement(ReorderableList list)
		{
			if (!CanAddOrRemove) return;
			// Don't just null it out, remove it!
			var count = list.count;
			ReorderableList.defaultBehaviours.DoRemoveButton(list);
			if (list.count == count)
			{
				ReorderableList.defaultBehaviours.DoRemoveButton(list);
			}
		}

		private void DrawHeader(Rect rect)
		{
			if (_foldable)
			{
				var header = new GUIContent($"▼ {_header}", List.serializedProperty.tooltip);
				if (GUI.Button(rect, header, EditorStyles.label)) HeaderClicked();
			}
			else
			{
				var header = new GUIContent($"{_header}", List.serializedProperty.tooltip);
				EditorGUI.LabelField(rect, header);
			}
			List.displayAdd = List.displayRemove = GUI.enabled;
		}
		
		private void HeaderClicked()
		{
			IsFolded = !IsFolded;
		}
		
		private void DrawElement(Rect rect, int i, bool isActive, bool isFocused)
		{
			if (IsFolded) return;
			var element = List.serializedProperty.GetArrayElementAtIndex(i);
			
			if (_linesPerElement > 1)
			{
				// Put a box around each element
				EditorGUI.HelpBox(rect, "", MessageType.None);
			}

			if (ShowIndex)
			{
				EditorGUI.LabelField(rect, $"[{i}]");
				rect.width -= 30f;
				rect.x += 30;
			}

			rect.y += 5;
			EditorGUI.PropertyField(rect, element, GUIContent.none, true);
		}

		private void ApplyChanges(ReorderableList list)
		{
			List.serializedProperty.serializedObject.ApplyModifiedProperties();
		}

		public void DoLayoutList(out bool isFolded)
		{
			DoLayoutList();
			isFolded = IsFolded;
		}

		
		public void DoLayoutList()
		{
			if (IsFolded)
			{
				var rect = GUILayoutUtility.GetRect(0.0f, List.headerHeight, GUILayout.ExpandWidth(true));
				if (Event.current.type == UnityEngine.EventType.Repaint)
				{
					((GUIStyle) "RL Header").Draw(rect, false, false, false, false);
				}

				if (GUI.Button(rect, $"  ► {_header}  -  {List.count} {(List.count == 1 ? "Item" : "Items")}", EditorStyles.label))
				{
					HeaderClicked();
				}
			}
			else
			{
				// if (!string.IsNullOrEmpty(_header) && !_foldable)
				// {
				// 	EditorTools.Header(_header);
				// }
				_scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, false, false);
				List.DoLayoutList();
				EditorGUILayout.EndScrollView();
			}
		}

		public int Count => List.serializedProperty.arraySize;

		public SerializedProperty GetElement(int index)
		{
			return List.serializedProperty.GetArrayElementAtIndex(index);
		}

		public SerializedProperty GetElement(int index, string property)
		{
			var obj = List.serializedProperty.GetArrayElementAtIndex(index);
			return obj.FindPropertyRelative(property);
		}

	}
}