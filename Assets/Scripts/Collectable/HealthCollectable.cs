using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthCollectable : Collectable
{
    [SerializeField] float healingValue;
    // Start is called before the first frame update
    void Start()
    {
        //REVIEW: remove this function if its not neeeded
    }
    //effect: heals player based on healing power of collectable.
    protected override void CollectableEffect()
    {
        playerBody.GetComponentInChildren<PlayerHealth>().Heal(healingValue);
    }
}
