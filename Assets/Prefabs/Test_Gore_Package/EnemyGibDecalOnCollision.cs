using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGibDecalOnCollision : MonoBehaviour
{
    ParticleSystem particle;
    List<ParticleCollisionEvent> theCollisionEvents;
    [SerializeField] GameObject bloodSplatterPrefab;
    [SerializeField] int amountOfSplats = 10;
    bool canSplatter = true;

    void Start()
    {
        particle = GetComponent<ParticleSystem>();
        theCollisionEvents = new List<ParticleCollisionEvent>();
    }

    //play sounds when particles hit ground
    void OnParticleCollision(GameObject other)
    {
        //get collision events
        int numCollisionEvents = particle.GetCollisionEvents(other, theCollisionEvents);

        amountOfSplats--;

        //Debug.Log("gib particle collided");
        if (canSplatter && amountOfSplats > 0)
        {
            

            //canSplatter = false;
            Instantiate(bloodSplatterPrefab, theCollisionEvents[0].intersection, Quaternion.FromToRotation(Vector3.forward, theCollisionEvents[0].intersection));
        }

    }
}
