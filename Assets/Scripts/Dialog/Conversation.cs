using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDialog", menuName = "ScriptableObjects /Dialog")]
public class Conversation : ScriptableObject
{
	[System.Serializable]
	public struct DialogLine
	{
		[Multiline]
		public string body;
		public bool changeCamera;
		public float CameraTransitionSpeed;
		public Vector3 CameraPos;
		public Vector3 CameraEuler;
	}
	public DialogLine[] script;

	public DialogLine this[int key]
	{
		get => script[key];
	}
}
