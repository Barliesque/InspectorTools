using UnityEngine;
using UnityEditor;

namespace Barliesque.InspectorTools.Editor
{
	[CustomPropertyDrawer(typeof(DisableAttribute))]
	public class DisableAttributeDrawer : PropertyDrawer
	{
		override public float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property, label, property.hasVisibleChildren);
		}
		
		override public void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (property == null) return;
			EditorGUI.BeginDisabledGroup(true);
			var content = new GUIContent(property.displayName, property.tooltip);
			EditorGUI.PropertyField(position, property, content, property.hasVisibleChildren);
			EditorGUI.EndDisabledGroup();
		}
	}
}