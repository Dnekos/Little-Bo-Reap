using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FrogAI : EnemyAI
{
	[Header("Explosion")]//this is our explosion
	[SerializeField] Transform ExplosionSpawnPoint;
	[SerializeField] float ExplosionDamage;

	// Start is called before the first frame update
	override protected void Start()
	{
		base.Start();
	}

	// for animation trigger
	public void SpawnShockwave()
	{
		if (activeAttack != null)
		{
			activeAttack.SpawnObject(ExplosionSpawnPoint.position, Quaternion.identity);
			activeAttack.damage = ExplosionDamage;
			OnDeath();//kills enemy
		}

		/*
		We still need to add explosion animation
		*/

	}
}