//REVIEW: Looks Good!

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealCollectable : Collectable
{
    public float healingValue;

    protected override void CollectableEffect()
    {
        //soulCounter.GetComponent<PlayerSoulCounter>().incrementSouls(soulValue);
        //Debug.Log("OverrideSuccessful");

        playerBody.GetComponentInChildren<PlayerHealth>().Heal(healingValue);
    }
}
