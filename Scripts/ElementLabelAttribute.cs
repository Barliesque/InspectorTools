using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class ElementLabelAttribute : PropertyAttribute
{
	public string MethodName { get; private set; }

	/// <summary>
	/// Apply custom labels to elements in an array in the Inspector.
	/// </summary>
	/// <param name="methodName">The name of a method in the element class that returns the string label.</param>
	public ElementLabelAttribute(string methodName = "ToString")
	{
		MethodName = methodName;
	}
}