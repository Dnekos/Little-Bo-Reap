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


	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	public override void Interact()
	{
		DB.ActivateUI(script);

	}
}
