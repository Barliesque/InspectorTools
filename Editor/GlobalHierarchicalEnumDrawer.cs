using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Barliesque.InspectorTools.Editor
{

    [CustomPropertyDrawer(typeof(Enum), true)]
    public class GlobalHierarchicalEnumDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Type enumType = fieldInfo.FieldType;
            if (enumType.IsArray) enumType = enumType.GetElementType();
            else if (enumType.IsGenericType) enumType = enumType.GetGenericArguments()[0];

            if (enumType == null || !enumType.IsEnum || enumType.IsDefined(typeof(FlagsAttribute), false))
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            string[] names = property.enumNames;
            GUIContent[] menuPaths = new GUIContent[names.Length];
            string[] buttonLabels = new string[names.Length];
            bool hasCustomPaths = false;

            for (int i = 0; i < names.Length; i++)
            {
                FieldInfo field = enumType.GetField(names[i]);
                var pathAttr = field?.GetCustomAttribute<EnumPathAttribute>();
                string defaultDisplayName = property.enumDisplayNames[i];

                if (pathAttr != null)
                {
                    hasCustomPaths = true;

                    // Determine the button label: use custom label if provided, otherwise standard display name
                    string resolvedLabel = !string.IsNullOrEmpty(pathAttr.Label)
                        ? pathAttr.Label
                        : defaultDisplayName;

                    buttonLabels[i] = resolvedLabel;

                    // Build the menu path using the category folder(s) + resolved label
                    string path = pathAttr.Path ?? string.Empty;
                    int lastSlash = path.LastIndexOf('/');
                    string menuPath = lastSlash >= 0
                        ? $"{path.Substring(0, lastSlash + 1)}{resolvedLabel}"
                        : resolvedLabel;

                    menuPaths[i] = new GUIContent(menuPath);
                }
                else
                {
                    menuPaths[i] = new GUIContent(defaultDisplayName);
                    buttonLabels[i] = defaultDisplayName;
                }
            }

            if (!hasCustomPaths)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);
            Rect controlRect = EditorGUI.PrefixLabel(position, label);

            int currentIndex = property.enumValueIndex;
            string currentButtonText = (!property.hasMultipleDifferentValues && currentIndex >= 0 && currentIndex < buttonLabels.Length)
                ? buttonLabels[currentIndex]
                : "Mixed";

            if (EditorGUI.DropdownButton(controlRect, new GUIContent(currentButtonText), FocusType.Keyboard))
            {
                GenericMenu menu = new GenericMenu();
                for (int i = 0; i < menuPaths.Length; i++)
                {
                    int index = i;
                    bool isSelected = !property.hasMultipleDifferentValues && index == currentIndex;
                    menu.AddItem(menuPaths[i], isSelected, () =>
                    {
                        property.serializedObject.Update();
                        property.enumValueIndex = index;
                        property.serializedObject.ApplyModifiedProperties();
                    });
                }

                menu.DropDown(controlRect);
            }

            EditorGUI.EndProperty();
        }
    }

}