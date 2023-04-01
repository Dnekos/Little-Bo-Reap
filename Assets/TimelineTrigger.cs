//REVIEW: Im not super familiar with using Playables, but after looking into the UnityEngine.Playables documentation, 
    //this looks nice and clean. 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


public class TimelineTrigger : MonoBehaviour
{
    [SerializeField] PlayableDirector timeline;
    [SerializeField] Animator babaAnimator;
    [SerializeField] string jumpEndClip;
    [SerializeField] float timeToJumpEnd;
    bool hasplayed = false;

    private void Start()
    {
        //this will freeze the animation, making it seem like the house is stationary. 
        babaAnimator.speed = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player") && !hasplayed)
        {
            babaAnimator.speed = 1;
            hasplayed = true;
            timeline.Play();
            StartCoroutine(JumpEnd());
        }
    }

    IEnumerator JumpEnd()
    {
        yield return new WaitForSeconds(timeToJumpEnd);
        babaAnimator.Play(jumpEndClip);
    }
}
