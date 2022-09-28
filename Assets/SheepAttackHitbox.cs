using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepAttackHitbox : MonoBehaviour
{
    [Header("The AI this hitbox is attatched to goes here")]
    [SerializeField] PlayerSheepAI sheepParent;
    Attack attack;

    private void OnEnable()
    {
        attack = sheepParent.attackBase;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Enemy"))
        {
            other?.GetComponent<EnemyAI>().TakeDamage(attack, sheepParent.transform.forward);
        }
    }
}
