using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceDialog : MonoBehaviour
{
	[SerializeField]
	Speaker targettedSpeaker;

	bool hasInteracted = false;
	private void OnTriggerEnter(Collider other)
	{
		if (!hasInteracted && other.gameObject == WorldState.instance.player)
		{
			targettedSpeaker.Interact();
			hasInteracted = true;
		}
	}
}
