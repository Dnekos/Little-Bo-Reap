using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Attack", menuName = "ScriptableObjects/Enemy Majority Attack", order = 2)]
public class MajorityAttack : EnemyAttack
{
	[Header("Attack Condition"), SerializeField]
	float PercentOfSheep = 0.5f;

	public override bool CheckCondition(Transform user, Transform player, List<Transform> NearbyGuys)
	{
		return NearbyGuys.Count >= player.GetComponent<PlayerSheepAbilities>().GetAverageActiveFlockSize() * PercentOfSheep;
	}
}
