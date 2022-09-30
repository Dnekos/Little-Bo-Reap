using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Attack", menuName = "ScriptableObjects/Enemy Attack", order = 2)]
public class EnemyAttack : Attack
{
	[Header("Enemy Specific"), Tooltip("Enemy will be unable to use this attack while coolingdown")]
	public float Cooldown;
}
