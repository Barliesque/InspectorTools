using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ElementLabelAttribute))]
public class ElementLabelAttributeDrawer : PropertyDrawer
{
	override public void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		var attr = (ElementLabelAttribute)attribute;
		var customLabel = label.text; 

		// Access the actual structural object instance of the array element
		var targetObject = property.boxedValue;

		if (targetObject != null)
		{
			// Extract the target method using reflection
			var method = targetObject.GetType().GetMethod(
				attr.MethodName, 
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
			);

			if (method != null)
			{
				// Invoke the method and capture its string return value
				var result = method.Invoke(targetObject, null);
				if (result != null)
				{
					customLabel = result.ToString();
				}
			}
			else
			{
				customLabel = $"[Error: Method '{attr.MethodName}' not found]";
			}
		}

		// Draw the native UI foldout with our generated text override
		EditorGUI.PropertyField(position, property, new GUIContent(customLabel), true);
	}

	override public float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return EditorGUI.GetPropertyHeight(property, label, true);
	}
}