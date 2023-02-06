using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonCollectable : Collectable
{
    protected override void CollectableEffect()
    {
        //soulCounter.GetComponent<PlayerSoulCounter>().incrementSouls(soulValue);
        //Debug.Log("OverrideSuccessful");

        playerBody.GetComponentInChildren<PlayerSummoningResource>().ResetBlood();
    }
}
