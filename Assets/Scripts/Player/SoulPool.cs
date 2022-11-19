using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulPool : MonoBehaviour
{
    [SerializeField] float soulsPerSec = 1f;

    private void OnTriggerStay(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            other.GetComponent<PlayerSummoningResource>()?.ChangeBloodAmount(soulsPerSec * Time.deltaTime);
        }
    }
}
