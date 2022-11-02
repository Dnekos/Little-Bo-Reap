using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackBase
{
	[SerializeField]
	string attackName;
	[SerializeField]
	Attack atk;


	[Header("SpawningHitbox")]
	[SerializeField]
	GameObject hitboxPrefab;
	[SerializeField]
	Transform spawnPoint;


	public bool CheckCondition()
	{
		return true;
	}

}
