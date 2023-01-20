using UnityEditor;
using UnityEngine;

namespace Barliesque.InspectorTools.Editor
{
	// Disabled due to not functioning properly when applied to arrays
	//[CustomPropertyDrawer(typeof(PrefabAttribute))]
	public class PrefabAttributeDrawer : PropertyDrawer
	{

		override public void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			position.height = 20f;
			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
			property.objectReferenceValue = EditorGUI.ObjectField(position, property.objectReferenceValue, fieldInfo.FieldType, false);
			EditorGUI.EndProperty();
		}
		
	}

}