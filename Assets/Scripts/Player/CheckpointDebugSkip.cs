using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointDebugSkip : MonoBehaviour
{
	[Header("Skip Options")]
	public int buildToSet = 10;
	public int ramToSet = 10;
	public int fluffyToSet = 10;
	bool hasAdded = false;

    private void OnEnable()
    {
		if(!hasAdded)
        {
			hasAdded = true;
			PlayerSheepAbilities player = WorldState.instance.player.GetComponent<PlayerSheepAbilities>();
			player.SetFlock(buildToSet, ramToSet, fluffyToSet);
		}
		
	}
}
