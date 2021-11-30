using UnityEngine;
using UnityEditor;

namespace Barliesque.InspectorTools.Editor
{

	/// <summary>
	/// This wizard will replace a selection with an object or prefab.
	/// Scene objects will be cloned (destroying their prefab links).
	/// Original coding by 'yesfish', nabbed from Unity Forums
	/// 'keep parent' added by Dave A (also removed 'rotation' option, using localRotation
	/// Updated for Unity 2019.4+ by David Barlia
	/// </summary>
	public class ReplaceSelection : ScriptableWizard
	{
		static private GameObject _replacement = null;
		static private bool _keep = false;

		public GameObject ReplacementObject = null;
		public bool KeepOriginals = false;

		[MenuItem("GameObject/Replace Selection...")]
		static private void CreateWizard()
		{
			DisplayWizard("Replace Selection", typeof(ReplaceSelection), "Replace");
		}

		public ReplaceSelection()
		{
			ReplacementObject = _replacement;
			KeepOriginals = _keep;
		}

		private void OnWizardUpdate()
		{
			_replacement = ReplacementObject;
			_keep = KeepOriginals;
		}

		private void OnWizardCreate()
		{
			if (_replacement == null)
				return;

			var transforms = Selection.GetTransforms(SelectionMode.TopLevel | SelectionMode.Editable);

			foreach (var t in transforms)
			{
				GameObject g;
				var pref = PrefabUtility.GetPrefabAssetType(_replacement);

				if (pref == PrefabAssetType.Regular || pref == PrefabAssetType.Model)
				{
					g = (GameObject)PrefabUtility.InstantiatePrefab(_replacement);
				}
				else
				{
					g = Instantiate(_replacement);
				}

				Undo.RegisterCreatedObjectUndo(g, "Added replacement prefab");

				var gTransform = g.transform;
				gTransform.parent = t.parent;
				g.name = _replacement.name;
				gTransform.localPosition = t.localPosition;
				gTransform.localScale = t.localScale;
				gTransform.localRotation = t.localRotation;
			}

			if (!_keep)
			{
				foreach (var g in Selection.gameObjects)
				{
					Undo.DestroyObjectImmediate(g);
				}
			}
		}
	}

}
