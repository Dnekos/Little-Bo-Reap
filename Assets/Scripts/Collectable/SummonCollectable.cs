using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonCollectable : Collectable
{
    [SerializeField] FMODUnity.EventReference collectableSFX;
    protected override void CollectableEffect()
    {
        //soulCounter.GetComponent<PlayerSoulCounter>().incrementSouls(soulValue);
        //Debug.Log("OverrideSuccessful");
        FMODUnity.RuntimeManager.PlayOneShot(collectableSFX, transform.position);
        playerBody.GetComponentInChildren<PlayerSummoningResource>().ResetBlood();
    }
}
