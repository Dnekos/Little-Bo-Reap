using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speaker : Interactable
{
	[Header("Speaker vars"), SerializeField]
	Transform inputIcon;

	public Conversation script;

	//[SerializeField] FMODUnity.EventReference talkSound;
	FMODUnity.StudioEventEmitter emitter;

	// prevent accidently clicking object when closing dialogue
	//public bool ClosedTextThisFrame = false;

	[SerializeField, Tooltip("Primary speechbubble object")]
	DialogBox DB; // speechbubble

	[Header("Repeating Dialog"), SerializeField, Tooltip("If filled, the repeating dialog will play instead of the normal script anytime after the first that the player interacts with it.")]
	Conversation repeatingScript;
	bool hasSpoken = false;

	Animator anim;
	Transform maincam;

	private void Start()
	{
		anim = GetComponentInChildren<Animator>();
		maincam = Camera.main.transform;
		inputIcon.gameObject.SetActive(false);
		emitter = GetComponent<FMODUnity.StudioEventEmitter>();
	}
	public override void Interact()
	{
		DB.ActivateUI(this);
		if (repeatingScript != null)
			script = repeatingScript;
	}
	private void Update()
	{
		inputIcon.rotation = maincam.rotation;//LookAt(WorldState.instance.player.transform.position, Vector3.up);
	}
	private void OnTriggerEnter(Collider other)
	{
		// if entered on the forced
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
		if (value && !emitter.IsPlaying())
			emitter.Play();
		else if (!value && emitter.IsPlaying())
			emitter.Stop();
		anim.SetBool("Talking", value);
	}

	// currently depricated
	public void FireSoundEvent()
	{
		//FMODUnity.RuntimeManager.PlayOneShotAttached(talkSound, gameObject);
	}
}
