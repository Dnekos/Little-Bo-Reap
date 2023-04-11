//REVIEW: Looks Good!
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulCollectableScript : Collectable
{
    [SerializeField] int soulValue;
    [SerializeField] FMODUnity.EventReference soulsfx;
    //effect: adds souls equal to value
    protected override void CollectableEffect()
    {
        Debug.Log("Add Souls: " + soulValue.ToString());
        WorldState.instance.PersistentData.soulsCount += soulValue;
        WorldState.instance.HUD.UpdateSoulCount();
        FMODUnity.RuntimeManager.PlayOneShot(soulsfx);
        //SheepPassives.soulsCount += soulValue;

        //clamps player soul count to a positive number.

        if (WorldState.instance.PersistentData.soulsCount < 0)
        {
            WorldState.instance.PersistentData.soulsCount = 0;
        }
    }
}
