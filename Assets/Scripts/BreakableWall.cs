using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableWall : MonoBehaviour
{
    [SerializeField] int health = 3;
    [SerializeField] GameObject gibs;

    public void DamageWall()
    {
        health--;
        if (health <= 0) BreakWall();
    }

    void BreakWall()
    {
        Instantiate(gibs, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
