using UnityEngine;

namespace Barliesque.InspectorTools
{
	public class ShowWithFlagAttribute : PropertyAttribute
	{
		public readonly string FlagsProp;
		public readonly object Flag;
		public readonly bool ShowIfSet;
		public readonly bool Indent;

		/// <summary>
		/// Show a property in the Inspector, only if an Enum property has a specified flag set.
		/// </summary>
		/// <param name="flagsProp">A property whose type is an enum with the [Flags] attribute</param>
		/// <param name="flag">The specific flag to control visibility of this property.</param>
		/// <param name="showIfSet">If true, the property will be visible if the flag is present.  If false, the flag hides the property.</param>
		/// <param name="indent">Indenting beneath the flags property helps show the connection.</param>
		public ShowWithFlagAttribute(string flagsProp, object flag, bool showIfSet = true, bool indent = true)
		{
			FlagsProp = flagsProp;
			Flag = flag;
			ShowIfSet = showIfSet;
			Indent = indent;
		}
	}
}