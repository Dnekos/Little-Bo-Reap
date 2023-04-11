using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(VerletVine))]
public class VineEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		VerletVine myScript = (VerletVine)target;

		if (GUILayout.Button("Build Vine"))
		{
			myScript.Simulate(myScript.InspectorTimeStep);
		}
		if (GUILayout.Button("Reset Points"))
		{
			myScript.CleanUp();
			myScript.Simulate(myScript.InspectorTimeStep);
		}
	}

	private void OnDisable()
	{
		VerletVine myScript = (VerletVine)target;
		myScript.CleanUp();
	}
}
