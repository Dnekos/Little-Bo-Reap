using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulCollectableScript : Collectable
{
    [SerializeField] int soulValue;
    [SerializeField] FMODUnity.EventReference soulSFX;
    //effect: adds souls equal to value
    protected override void CollectableEffect()
    {
        Debug.Log("Add Souls: " + soulValue.ToString());
        FMODUnity.RuntimeManager.PlayOneShot(soulSFX, transform.position);
        WorldState.instance.passiveValues.soulsCount += soulValue;

        //clamps player soul count to a positive number.
        if (WorldState.instance.passiveValues.soulsCount < 0)
        {
            WorldState.instance.passiveValues.soulsCount = 0;
        }
    }
}
