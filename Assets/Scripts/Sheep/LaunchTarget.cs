using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchTarget : PuzzleKey
{
    [SerializeField] string hitAnimation;
    [SerializeField] FMODUnity.EventReference hit;
    public void OpenKey()
    {
        if (!isOpened)
        {
            GetComponent<Animator>().Play(hitAnimation);
			isOpened = true;
            FMODUnity.RuntimeManager.PlayOneShot(hit);
            Debug.Log(gameObject.name+ " was hit!");
        }
    }
}
