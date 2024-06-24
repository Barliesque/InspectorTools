using UnityEngine;
using UnityEditor;

namespace Barliesque.InspectorTools.Editor
{
	[CustomPropertyDrawer(typeof(DisableIfAttribute))]
	public class DisableIfAttributeDrawer : PropertyDrawer
	{

		override public void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var a = (DisableIfAttribute)attribute;
			SerializedProperty t = property.serializedObject.FindProperty(a.Target);

			bool disable = false;
			if (t == null) Debug.LogWarning("[DisableIf] Invalid Property Name for Attribute.", property.serializedObject.targetObject);
			else disable = t.boolValue == a.DisabledState;
			
			if (disable) EditorGUI.BeginDisabledGroup(true);
			var content = new GUIContent(property.displayName, property.tooltip);
			EditorGUI.PropertyField(position, property, content, property.hasVisibleChildren);
			if (disable) EditorGUI.EndDisabledGroup();
		}
	}
}