using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    [SerializeField] int health = 3;
    [SerializeField] GameObject gibs;

    [Header("Sounds")]
    [SerializeField] FMODUnity.EventReference breakSound;

    public void DamageWall()
    {
        health--;
        if (health <= 0) BreakWall();
    }

    void BreakWall()
    {
        FMODUnity.RuntimeManager.PlayOneShot(breakSound, transform.position);
        GameObject destroyedWall = Instantiate(gibs, transform.position, transform.rotation);
        destroyedWall.GetComponentInChildren<Rigidbody>().AddExplosionForce(20f, transform.position, 5f);
        
        Destroy(gameObject);

    }


}