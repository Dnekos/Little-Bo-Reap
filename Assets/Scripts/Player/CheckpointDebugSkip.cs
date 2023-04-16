using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointDebugSkip : MonoBehaviour
{
	[Header("Skip Options")]
	public int buildToAdd = 10;
	public int ramToAdd = 10;
	public int fluffyToAdd = 10;
	bool hasAdded = false;

    private void OnEnable()
    {
		if(!hasAdded)
        {
			hasAdded = true;
			PlayerSheepAbilities player = WorldState.instance.player.GetComponent<PlayerSheepAbilities>();
			player.AddToFlock(buildToAdd, ramToAdd, fluffyToAdd);
		}
		
	}
}
