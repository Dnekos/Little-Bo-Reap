using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadRingerCollectable : Collectable
{
    protected override void CollectableEffect()
    {
        playerBody.GetComponentInChildren<PlayerGothMode>().AddToGothMeter(1f);
    }
}
