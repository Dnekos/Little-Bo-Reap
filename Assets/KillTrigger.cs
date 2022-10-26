using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillTrigger : MonoBehaviour
{
    [SerializeField] Attack killAttack;

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<EnemyAI>()!= null || other.GetComponent<PlayerSheepAI>()!= null)
        {
            other.GetComponent<Damageable>()?.ForceKill();
        }
    }
}
