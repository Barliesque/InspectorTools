using UnityEngine;
using UnityEditor;

namespace Barliesque.InspectorTools.Editor
{
	[CustomPropertyDrawer(typeof(DisableIfUnassignedAttribute))]
	public class DisableIfUnassignedAttributeDrawer : PropertyDrawer
	{
		override public float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return IsDisabled(property) ? 0f : base.GetPropertyHeight(property, label);
		}

		override public void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (Indent) ++EditorGUI.indentLevel;
			
			var content = new GUIContent(property.displayName, property.tooltip);
			var wasEnabled = GUI.enabled;
			var disable = IsDisabled(property);
			
			GUI.enabled = wasEnabled && !disable;
			EditorGUILayout.PropertyField(property, content, property.hasVisibleChildren);
			GUI.enabled = wasEnabled;
			
			if (Indent) --EditorGUI.indentLevel;
		}

		private bool IsDisabled(SerializedProperty property)
		{
			var attr = (DisableIfUnassignedAttribute)attribute;
			var target = property.serializedObject.FindProperty(attr.Target);

			bool disable = false;
			if (target == null) Debug.LogWarning($"[DisableIfUnassigned] Invalid Property Name for Attribute: {attr.Target}");
			else if (target.objectReferenceValue == null) disable = true;
			
			if (attr.Reverse) return !disable;
			return disable;
		}

		private bool Indent => ((DisableIfUnassignedAttribute)attribute).Indent;
		
	}
}