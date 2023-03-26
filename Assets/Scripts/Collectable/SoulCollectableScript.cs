using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulCollectableScript : Collectable
{
    [SerializeField] int soulValue;
    //effect: adds souls equal to value
    protected override void CollectableEffect()
    {
        Debug.Log("Add Souls: " + soulValue.ToString());
        WorldState.instance.PersistentData.soulsCount += soulValue;
        WorldState.instance.HUD.UpdateSoulCount(WorldState.instance.PersistentData.soulsCount.ToString());
        //SheepPassives.soulsCount += soulValue;

        //clamps player soul count to a positive number.
        
        if (WorldState.instance.PersistentData.soulsCount < 0)
        {
            WorldState.instance.PersistentData.soulsCount = 0;
        }
    }
}
