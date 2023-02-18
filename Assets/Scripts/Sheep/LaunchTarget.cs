using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchTarget : MonoBehaviour
{
    [SerializeField] string hitAnimation;
    public bool targetHit = false;
    public void DoThing()
    {
        if (!targetHit)
        {
            GetComponent<Animator>().Play(hitAnimation);
            targetHit = true;
            Debug.Log("launch target did a thing!");
        }
    }
}
