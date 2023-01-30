using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speaker : Interactable
{
	public Conversation script;

	// prevent accidently clicking object when closing dialogue
	public bool ClosedTextThisFrame = false;

	[SerializeField, Tooltip("Primary speechbubble object")]
	DialogBox DB; // speechbubble

	Animator anim;

	private void Start()
	{
		anim = GetComponentInChildren<Animator>();
	}
	public override void Interact()
	{
		DB.ActivateUI(this);
	}

	public void SetTalking(bool value)
	{
		anim.SetBool("Talking", value);
	}
}
