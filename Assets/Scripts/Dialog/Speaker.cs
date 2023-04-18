using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speaker : Interactable
{
	[Header("Speaker vars"), ]
	public Conversation script;

	//[SerializeField] FMODUnity.EventReference talkSound;
	FMODUnity.StudioEventEmitter emitter;

	// prevent accidently clicking object when closing dialogue
	//public bool ClosedTextThisFrame = false;

	[SerializeField, Tooltip("Primary speechbubble object")]
	DialogBox DB; // speechbubble

	[Header("Repeating Dialog"), SerializeField, Tooltip("If filled, the repeating dialog will play instead of the normal script anytime after the first that the player interacts with it.")]
	Conversation repeatingScript;

	Animator anim;

	[SerializeField] bool isFinalDialogue;

	protected override void Start()
	{
		base.Start();

		anim = GetComponentInChildren<Animator>();
		emitter = GetComponent<FMODUnity.StudioEventEmitter>();
		if (DB == null) //Dialogue box wasn't properly set/this object was spawned later and needs to find one
		{
			//Debug.Log("Attempted to get Dialogue Box. Number of Children = " + WorldState.instance.player.transform.parent.gameObject.transform.GetChild(6).name.ToString());
            //this is such fucking bad implementation I'm so sorry Demetri
            DB = WorldState.instance.player.transform.parent.gameObject.transform.GetChild(6).GetChild(0).GetComponent<DialogBox>();
			//DB.SetFinalDialogue(isFinalDialogue);
		}
	}
	public override void Interact()
	{
		DB.ActivateUI(this);

		if (repeatingScript != null)
			script = repeatingScript;
	}
	
	public void SetTalking(bool value)
	{
		if (emitter != null)
		{
            if (value && !emitter.IsPlaying())
                emitter.Play();
            else if (!value && emitter.IsPlaying())
                emitter.Stop();
            anim.SetBool("Talking", value);
        }
	}

	// currently depricated
	public void FireSoundEvent()
	{
		//FMODUnity.RuntimeManager.PlayOneShotAttached(talkSound, gameObject);
	}
}
