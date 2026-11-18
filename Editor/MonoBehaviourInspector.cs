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
			EditorTools.DrawInspectorButtons(target, targets);
		}
	}

}