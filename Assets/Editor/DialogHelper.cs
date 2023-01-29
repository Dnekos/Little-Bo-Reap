using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Conversation.DialogLine))]
public class DialogHelper : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		if (!property.isExpanded)
			return EditorGUIUtility.singleLineHeight;
		return 165;
	}
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.PropertyField(position, property, label, true);

		if (property.isExpanded && GUI.Button(new Rect(position.x, position.yMax-20,position.width,20), "Set Camera to Scene View"))
		{
			property.FindPropertyRelative("CameraPos").vector3Value = SceneView.lastActiveSceneView.camera.transform.position;
			property.FindPropertyRelative("CameraEuler").vector3Value = SceneView.lastActiveSceneView.camera.transform.rotation.eulerAngles;
		}
	}

}
