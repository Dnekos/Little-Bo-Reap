//REVIEW: Im not super familiar with using Playables, but after looking into the UnityEngine.Playables documentation, 
    //this looks nice and clean. 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


public class TimelineTrigger : MonoBehaviour
{
    [SerializeField] PlayableDirector timeline;
    bool hasplayed = false;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player") && !hasplayed)
        {
            hasplayed = true;
            timeline.Play();
        }
    }
}
