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

	bool isSuspended = false;

	float bossFallRate = 2000;

	Vector3 spawnPoint;
 

	// Start is called before the first frame update
	override protected void Start()
	{
		base.Start();

		spawnPoint = transform.position;
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
			StartCoroutine(DelayMovement());
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
			StartCoroutine(DelayMovement());
		}
	}

	public void RangedAttack()
    {
		if (activeAttack != null)
		{
			activeAttack.SpawnObject(rangedAttackSpawnPoint.position, rangedAttackSpawnPoint.rotation);
			activeAttack.damage = rangedAttackDamage;
			StartCoroutine(DelayMovement());
		}

	}

    public override void TakeDamage(Attack atk, Vector3 attackForward, float damageAmp = 1, float knockbackMultiplier = 1)
    {
        base.TakeDamage(atk, attackForward, damageAmp, 0.0f);//no knockback
    }

	public override bool SetDestination(Vector3 dest)
    {
		// dont pathfind bad destinations
		if (dest == null || float.IsNaN(dest.x))
		{
			Debug.LogWarning("tried giving " + gameObject + " invalid destination");
			return false;
		}
		if (!GetAgent().isOnNavMesh && !GetAgent().isOnOffMeshLink)
		{
			print(gameObject + " failed to find a destination");
			//base.OnDeath();
			return false;
		}
		else
		{
			GetAgent().SetDestination(dest);
			if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5, 1) || GetAgent().isOnOffMeshLink)
			{
				transform.position = hit.position;
			}
			else
			{
				print(gameObject + " tried finding a destination while not on a valid point");
				//base.OnDeath();
				return false;
			}
		}
		
		//StartCoroutine(DelayMovement());
		return true;
	}
	
	IEnumerator DelayMovement()
    {
		//MoveBoss();
		yield return new WaitForSeconds(1);
		MoveBoss();
		yield return new WaitForSeconds(1);

	}

	public void MoveBoss()
    {
		//for now, a predetermined vector
		Vector3 moveToPos = RandomPointInCircle(spawnPoint, 50f);

		SetDestination(moveToPos);

    }

	Vector3 RandomPointInCircle(Vector3 center, float radius)
    {
		Vector3 randomPosition = UnityEngine.Random.insideUnitSphere * radius;
		Vector3 destinationPosition = new Vector3(randomPosition.x + center.x, center.y, randomPosition.z + center.z);

		Vector3 result = destinationPosition;
		//calculate
		//Debug.Log(result);

		return result;

	}
}