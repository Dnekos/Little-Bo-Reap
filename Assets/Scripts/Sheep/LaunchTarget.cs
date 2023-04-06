using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchTarget : PuzzleKey
{
    [SerializeField] string hitAnimation;
    public void OpenKey()
    {
        if (!isOpened)
        {
            GetComponent<Animator>().Play(hitAnimation);
			isOpened = true;
            Debug.Log(gameObject.name+ " was hit!");
        }
    }
}
