using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
	[SerializeField]
	Animation anim;

	public Transform RespawnPoint;

	[SerializeField] FMODUnity.EventReference Sound;

	bool hasEntered = false; // prevents the same checkpoint from being triggered twice

	[Header("CheckpointSkip")]
	public bool addsSheep;
	public GameObject debugSheepAdder;	

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player" && !hasEntered)
		{
			Debug.Log(other.gameObject + " hit checkpoint at " + transform.position);
			WorldState.instance.SetSpawnPoint(this);

			FMODUnity.RuntimeManager.PlayOneShotAttached(Sound, gameObject);

			hasEntered = true;
			anim.Play();
		}
	}
}
