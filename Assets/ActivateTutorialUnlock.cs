using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateTutorialUnlock : MonoBehaviour
{
	[SerializeField]
	SaveData.TutorialUnlocks unlockedFeature;


	bool hasInteracted = false;
	private void OnTriggerEnter(Collider other)
	{
		if (!hasInteracted && other.gameObject == WorldState.instance.player)
		{
			WorldState.instance.PersistentData.unlocks |= unlockedFeature;
			hasInteracted = true;
		}
	}
}
