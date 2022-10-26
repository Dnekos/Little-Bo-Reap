using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepAttackHitbox : MonoBehaviour
{
    [Header("The AI this hitbox is attatched to goes here")]
    [SerializeField] PlayerSheepAI sheepParent;
    SheepAttack attack;

    private void OnEnable()
    {
        attack = sheepParent.attackBase;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Enemy"))
        {
            //if black sheep use black sheep damage overload, else attack normally
            if(sheepParent.isBlackSheep)
            {
                Instantiate(attack.explosionEffect, transform.position, transform.rotation);
                other?.GetComponent<EnemyAI>().TakeDamage(attack, sheepParent.transform.forward);

                sheepParent.TakeDamage(sheepParent.selfDamage, transform.forward);
            }
            else other?.GetComponent<EnemyAI>().TakeDamage((Attack)attack, sheepParent.transform.forward);
        }
    }
}
