using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Sheep Attack", menuName = "ScriptableObjects/Sheep Attack", order = 3)]
public class SheepAttack : Attack
{
    [Header("Black Sheep Attack Variables")]
    public float damageBlack;
    public float forwardKnockbackBlack;
    public float upwardKnockbackBlack;
    public bool dealsHitstunBlack = true;
    public GameObject explosionEffect;
}
