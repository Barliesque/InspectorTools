using UnityEngine;
using UnityEditor;

namespace Barliesque.InspectorTools.Editor
{
	[CustomPropertyDrawer(typeof(DisableAttribute))]
	public class DisableAttributeDrawer : PropertyDrawer
	{
		override public float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return base.GetPropertyHeight(property, label) - 20f;
		}

		override public void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.PropertyField(property, new GUIContent(property.displayName, property.tooltip), property.hasVisibleChildren);
			EditorGUI.EndDisabledGroup();
		}
	}
}