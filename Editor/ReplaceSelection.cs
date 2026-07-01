using UnityEditor;
using UnityEngine;

namespace Barliesque.InspectorTools.Editor
{

	/// <summary>
	/// This allows the Replace Selection tool to be opened via context menu click from the Hierarchy
	/// </summary>
	public class ReplaceSelection : ScriptableWizard
	{
		static private ReplaceSelectionWindow _window;

		[MenuItem("GameObject/Replace Selection...", priority = 100)]
		static private void OpenTool()
		{
			if (_window)
			{
				_window.Focus();
			}
			else
			{
				_window = GetWindow<ReplaceSelectionWindow>("Replace Selection");
				_window.minSize = new Vector2(300, 150);
			}
		}

	}

}
