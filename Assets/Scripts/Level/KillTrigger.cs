using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillTrigger : MonoBehaviour
{
    [SerializeField] Attack killAttack;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<EnemyAI>() != null || other.GetComponent<PlayerSheepAI>() != null)
        {
            other.GetComponent<Damageable>()?.ForceKill();
        }

        if (other.GetComponent<PlayerHealth>() != null)
        {
            other.GetComponent<Damageable>().TakeDamage(killAttack, transform.forward);
        }
    }
}
