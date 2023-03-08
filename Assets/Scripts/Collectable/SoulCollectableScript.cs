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
        //WorldState.instance.passiveValues.soulsCount += soulValue;
        SheepPassives.soulsCount += soulValue;

        //clamps player soul count to a positive number.
        /*
        if (WorldState.instance.passiveValues.soulsCount < 0)
        {
            WorldState.instance.passiveValues.soulsCount = 0;
        }
        */

        if (SheepPassives.soulsCount < 0)
        {
            SheepPassives.soulsCount = 0;
        }
    }
}
