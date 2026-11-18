using UnityEditor;
using UnityEngine;

namespace Barliesque.InspectorTools.Editor
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(MonoBehaviour), true)]
	public class MonoBehaviourInspector : UnityEditor.Editor
	{
		override public void OnInspectorGUI()
		{
			DrawDefaultInspector();
			
			EditorTools.DrawUILine(Color.gray, 2, 16);
			EditorTools.DrawInspectorButtons(target, targets);
		}
	}

}