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
        playerBody.GetComponent<PlayerProgressionHolder>().incrementSouls(soulValue);
    }
}
