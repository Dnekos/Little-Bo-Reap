using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetDoor : MonoBehaviour
{
    [SerializeField] List<LaunchTarget> targets;
    bool hasOpened;
    private void Update()
    {
        //for now use this
        bool openDoor = true;

        for (int i = 0; i < targets.Count; i++)
        {
            if (!targets[i].targetHit) openDoor = false;
        }

        //for now, destroy door. can have an animation or something more pretty later
        if (openDoor && !hasOpened)
        {
            hasOpened = true;
            Destroy(gameObject);
        }
    }
}
