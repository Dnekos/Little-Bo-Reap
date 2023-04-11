using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadRingerCollectable : Collectable
{
    [SerializeField] FMODUnity.EventReference collectableSFX;
    protected override void CollectableEffect()
    {
        playerBody.GetComponentInChildren<PlayerGothMode>().AddToGothMeter(1f);
        FMODUnity.RuntimeManager.PlayOneShot(collectableSFX, transform.position);
    }
}
