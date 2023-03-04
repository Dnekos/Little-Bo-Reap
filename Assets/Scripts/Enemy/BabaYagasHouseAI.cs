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

	[Header("Game End Stuff")]
	[SerializeField] GameObject endGameObject;



	float bossFallRate = 2000;
 

	// Start is called before the first frame update
	override protected void Start()
	{
		base.Start();


	}

	private void FixedUpdate()
    {
		rb.AddForce(Vector3.down * bossFallRate);
	}



	protected override void OnDeath()
    {
		//spawn the end house! :D
		Instantiate(endGameObject, transform.position, Quaternion.Euler(Vector3.zero));

        base.OnDeath();
    }

    // for animation trigger
    public void SpawnShockwave()//this will be the Slam attack
	{
		if (activeAttack != null)
		{
			activeAttack.SpawnObject(StompSpawnPoint.position, StompSpawnPoint.rotation);
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
			activeAttack.SpawnObject(enemySpawnPoint.position, enemySpawnPoint.rotation);
			enemiesSpawned = true;
		}

	}
	public void BreatheFire()
    {
		if (activeAttack != null)
		{
			activeAttack.SpawnObject(fireBreathSpawnPoint.position, fireBreathSpawnPoint.rotation);
			activeAttack.damage = fireBreathDamage;
		}
	}

	public void RangedAttack()
    {
		if (activeAttack != null)
		{
			activeAttack.SpawnObject(rangedAttackSpawnPoint.position, rangedAttackSpawnPoint.rotation);
			activeAttack.damage = rangedAttackDamage;
		}

	}

    public override void TakeDamage(Attack atk, Vector3 attackForward, float damageAmp = 1, float knockbackMultiplier = 1)
    {
        base.TakeDamage(atk, attackForward, damageAmp, 0.0f);//no knockback
    }

}