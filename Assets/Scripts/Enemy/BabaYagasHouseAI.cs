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
	[SerializeField] bool enemiesSpawned = false;
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
			activeAttack.SpawnObject(StompSpawnPoint);
			activeAttack.damage = StompDamage;
		}

	}

	public float GetHeath()
    {
		return Health;
    }
	public float GetMaxHeath()
	{
		return MaxHealth;
	}

	public bool getEnemiesSpawned()
    {
		return enemiesSpawned;
    }

	public void SpawnEnemies()
	{
		if (activeAttack != null)
		{
			activeAttack.SpawnObject(enemySpawnPoint);
		}

	}
	public void BreatheFire()
    {
		if (activeAttack != null)
		{
			activeAttack.SpawnObject(fireBreathSpawnPoint);
			activeAttack.damage = fireBreathDamage;
		}
	}

	public void RangedAttack()
    {
		if (activeAttack != null)
		{
			activeAttack.SpawnObject(rangedAttackSpawnPoint);
			activeAttack.damage = rangedAttackDamage;
		}

	}

}