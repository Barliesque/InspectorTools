using UnityEngine;

namespace Barliesque.InspectorTools
{
	public class DisableIfAttribute : PropertyAttribute
	{
		public string Target;
		public bool DisabledState;

		public DisableIfAttribute(string targetProperty, bool disabledState)
		{
			Target = targetProperty;
			DisabledState = disabledState;
		}
	}
}