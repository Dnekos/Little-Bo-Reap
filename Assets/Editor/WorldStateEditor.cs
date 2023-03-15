using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(WorldState))]
public class WorldStateEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		serializedObject.Update();

		WorldState myScript = (WorldState)target;

		if (GUILayout.Button("List Graves"))
		{
			SerializedProperty graves = serializedObject.FindProperty("SheepGraves");
			 
			myScript.SheepGraves = FindObjectsOfType<SheepGrave>();
			Debug.Log("Logged graves, found " + myScript.SheepGraves.Length +" graves.");
		}
		if (GUILayout.Button("List Doors"))
		{
			myScript.doors = FindObjectsOfType<PuzzleDoor>();
			Debug.Log("Logged graves, found " + myScript.doors.Length + " graves.");
		}

		//for each property you want to draw ....
		//EditorGUILayout.PropertyField(serializedObject.FindProperty("SheepGraves").InsertArrayElementAtIndex);

		//if you need to do something cute like use a different input type you can do this kind of thing...
		//SerializedProperty specialProp = serializedObject.FindProperty("myFloatThatPretendsItsAnInt");
		//specialProp.floatValue = EditorGUILayout.IntField(specialProp.floatValue as int) as float;

		//do this last!  it will loop over the properties on your object and apply any it needs to, no if necessary!
		//serializedObject.ApplyModifiedProperties();
		if (GUI.changed)
		{
			EditorUtility.SetDirty(myScript);
			EditorSceneManager.MarkSceneDirty(myScript.gameObject.scene);
		}

	}
}
