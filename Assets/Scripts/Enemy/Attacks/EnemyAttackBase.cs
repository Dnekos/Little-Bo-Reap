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

	[Header("Cooldown")]
	[SerializeField] float Cooldown = 3;
	float cooldownTimer = 0;


	public virtual void Update()
	{
		cooldownTimer -= Time.deltaTime;
	}

	public virtual bool CheckCondition(Transform user, List<Transform> NearbyGuys)
	{
		return cooldownTimer <= 0;
	}
	public virtual void PerformAttack(Animator anim)
	{
		anim.Play(atk.animation);
		cooldownTimer = Cooldown;

	}

}
