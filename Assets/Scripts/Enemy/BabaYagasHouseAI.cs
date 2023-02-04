using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BabaYagasHouseAI : EnemyAI
{
	[Header("Stomp Attack")]//this is our explosion
	[SerializeField] Transform StompSpawnPoint;
	[SerializeField] float StompDamage;

	[Header("Spawning Enemies")]
	[SerializeField] int numEnemiesSpawned;
	[SerializeField] Transform enemySpawnPoint;
	//include which enemies will spawn 

	[Header("Fire Breath")]
	[SerializeField] float fireBreathDamage;
	[SerializeField] Transform fireBreathSpawnPoint;

	[Header("Ranged Attack")]
	[SerializeField] float rangedAttackDamage;
    [SerializeField] Transform rangedAttackSpawnPoint;
 

	// Start is called before the first frame update
	override protected void Start()
	{
		base.Start();
	}

	// for animation trigger
	public void SpawnShockwave()//this will be the Slam attack
	{
		if (activeAttack != null)
		{
			activeAttack.SpawnObject(StompSpawnPoint.position);
			activeAttack.damage = StompDamage;
		}

	}

	public void SpawnEnemies()
    {

    }

	public void BreatheFire()
    {

    }

	public void RangedAttack()
    {

    }

}