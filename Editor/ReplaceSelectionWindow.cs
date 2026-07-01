using UnityEngine;
using UnityEditor;

namespace Barliesque.InspectorTools.Editor
{
    public class ReplaceSelectionWindow : EditorWindow
    {
        private GameObject _replacementObject;
        private bool _keepOriginals;

        [MenuItem("GameObject/Replace Selection...", false, 100)]
        static private void ShowWindow()
        {
            var window = GetWindow<ReplaceSelectionWindow>("Replace Selection");
            window.minSize = new Vector2(300, 150);
        }

        private void OnGUI()
        {
            EditorTools.HelpBox(
                "This tool will replace a selection with an object or prefab.  " +
                "Scene objects will be cloned (destroying their prefab links).", MessageType.Info);
            
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
            if (!_replacementObject)
            {
                return;
            }

            // Get TopLevel selection to avoid replacing both a parent and its child
            var selectedTransforms = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.Editable);

            if (selectedTransforms.Length == 0)
            {
                Debug.LogWarning("Replace Selection: No objects selected to replace.");
                return;
            }

            Undo.SetCurrentGroupName($"Replace {selectedTransforms.Length} Objects");
            var group = Undo.GetCurrentGroup();

            foreach (var targetTransform in selectedTransforms)
            {
                GameObject newObject;
                var assetType = PrefabUtility.GetPrefabAssetType(_replacementObject);

                // If it's a prefab asset, maintain the link; otherwise, clone the object
                if (assetType is PrefabAssetType.Regular or PrefabAssetType.Model)
                {
                    newObject = (GameObject)PrefabUtility.InstantiatePrefab(_replacementObject);
                }
                else
                {
                    newObject = Instantiate(_replacementObject);
                }

                if (!newObject) continue;

                Undo.RegisterCreatedObjectUndo(newObject, "Instantiate Replacement");

                var newTransform = newObject.transform;
                newTransform.parent = targetTransform.parent;
                newTransform.localPosition = targetTransform.localPosition;
                newTransform.localRotation = targetTransform.localRotation;
                newTransform.localScale = targetTransform.localScale;
                
                newObject.name = _replacementObject.name;
            }

            if (!_keepOriginals)
            {
                foreach (var targetGameObject in Selection.gameObjects)
                {
                    Undo.DestroyObjectImmediate(targetGameObject);
                }
            }

            Undo.CollapseUndoOperations(group);
        }
    }
}