using UnityEngine;

namespace Barliesque.InspectorTools
{
	public class DisableIfUnassignedAttribute : PropertyAttribute
	{
		public readonly string Target;
		public readonly bool Reverse;
		public readonly bool Indent;

		/// <summary>
		/// Disable this property in the Inspector, if another property does not have an object value assigned.
		/// </summary>
		/// <param name="targetProperty">Name of another property which must have an object assignment in order for this property to be enabled.</param>
		/// <param name="reverse">If true, then the target property must *not* be assigned for this property to be enabled in the Inspector.</param>
		/// <param name="indent">Optionally indent this property in the Inspector.</param>
		public DisableIfUnassignedAttribute(string targetProperty, bool reverse = false, bool indent = false)
		{
			Target = targetProperty;
			Reverse = reverse;
			Indent = indent;
		}
	}
}