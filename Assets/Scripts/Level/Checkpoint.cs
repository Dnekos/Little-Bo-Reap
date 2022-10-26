using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
	public Transform RespawnPoint;


	bool hasEntered = false; // prevents the same checkpoint from being triggered twice

	private void Start()
	{
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player" && !hasEntered)
		{
			Debug.Log(other.gameObject + " hit checkpoint at " + transform.position);
			WorldState.instance.SetSpawnPoint(this);
			hasEntered = true;
		}
	}
}
