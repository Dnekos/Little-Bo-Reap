using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speaker : Interactable
{
	[Header("Speaker vars"), SerializeField]
	Transform inputIcon;

	public Conversation script;

	// prevent accidently clicking object when closing dialogue
	public bool ClosedTextThisFrame = false;

	[SerializeField, Tooltip("Primary speechbubble object")]
	DialogBox DB; // speechbubble

	Animator anim;
	Transform maincam;

	private void Start()
	{
		anim = GetComponentInChildren<Animator>();
		maincam = Camera.main.transform;
	}
	public override void Interact()
	{
		DB.ActivateUI(this);
	}
	private void Update()
	{
		inputIcon.rotation = maincam.rotation;//LookAt(WorldState.instance.player.transform.position, Vector3.up);
	}
	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject == WorldState.instance.player)
			inputIcon.gameObject.SetActive(true);

	}
	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject == WorldState.instance.player)
			inputIcon.gameObject.SetActive(false);
	}
	public void SetTalking(bool value)
	{
		anim.SetBool("Talking", value);
	}
}
