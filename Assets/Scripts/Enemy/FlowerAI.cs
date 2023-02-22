using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerAI : Damageable
{
    [SerializeField] Transform flowerBody;
    [SerializeField] Transform flowerHead;

    [SerializeField] Transform attackPoint;

    [Header("Sounds")]
    [SerializeField] FMODUnity.EventReference attackSound;

    [HideInInspector]
    public Transform player;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();

        player = WorldState.instance.player.transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
