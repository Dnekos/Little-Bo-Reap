using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillTrigger : MonoBehaviour
{
    [SerializeField] Attack killAttack;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyAI>()?.ForceKill();
        }
        if(other.CompareTag("PlayerSheep"))
        {
            other.GetComponent<PlayerSheepAI>()?.ForceKill();
        }
    }
}
