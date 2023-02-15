using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;

    [SerializeField] int health = 3;
    [SerializeField] private GameObject brokenObjectPrefab;
    [SerializeField] private Transform explosionOrigin;//this could go eventually, just testing something out

    [Header("Sounds")]
    [SerializeField] FMODUnity.EventReference breakSound;

    public void DamageWall()
    {
        health--;
        if (health <= 0) Destroy();
    }

    public void Destroy()
    {
        FMODUnity.RuntimeManager.PlayOneShot(breakSound, transform.position);
        GameObject destroyedObject = Instantiate(brokenObjectPrefab, transform.position, transform.rotation);

        Rigidbody[] brokenPieces = destroyedObject.GetComponentsInChildren<Rigidbody>();

        //Apply the explosive force to each Shattered Piece individually.
        foreach (Rigidbody childRigidbody in brokenPieces)
        {
            childRigidbody.AddExplosionForce(600f, explosionOrigin.position, 500f);
        }

        Destroy(gameObject);
    }

}