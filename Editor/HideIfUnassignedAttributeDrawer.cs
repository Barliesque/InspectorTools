using UnityEngine;
using UnityEditor;

namespace Barliesque.InspectorTools.Editor
{
	[CustomPropertyDrawer(typeof(HideIfUnassignedAttribute))]
	public class HideIfUnassignedAttributeDrawer : PropertyDrawer
	{
		override public float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return IsHidden(property) ? 0f : base.GetPropertyHeight(property, label);
		}

		override public void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (IsHidden(property)) return;
			if (Indent) ++EditorGUI.indentLevel;
			var content = new GUIContent(property.displayName, property.tooltip);
			EditorGUI.PropertyField(position, property, content, property.hasVisibleChildren);
			if (Indent) --EditorGUI.indentLevel;
		}

		private bool IsHidden(SerializedProperty property)
		{
			var a = (HideIfUnassignedAttribute)attribute;
			SerializedProperty t = property.serializedObject.FindProperty(a.Target);

			bool Hide = false;
			if (t == null) Debug.LogWarning("[HideIfUnassigned] Invalid Property Name for Attribute.", property.serializedObject.targetObject);
			else Hide = !t.objectReferenceValue;
			return Hide;
		}

		private bool Indent => ((HideIfUnassignedAttribute)attribute).Indent;
		
	}
}