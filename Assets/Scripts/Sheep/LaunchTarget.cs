using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchTarget : MonoBehaviour
{

    public bool targetHit = false;
    public void DoThing()
    {
        targetHit = true;
        Debug.Log("launch target did a thing!");
    }
}
