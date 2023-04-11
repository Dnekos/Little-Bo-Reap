using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleStateOnEnableHUD : MonoBehaviour
{
    [SerializeField] string idleAnimation;

    private void OnEnable()
    {
        GetComponent<Animator>().Play(idleAnimation);
    }
}
