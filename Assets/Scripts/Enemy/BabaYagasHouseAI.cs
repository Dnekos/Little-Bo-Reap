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
    [HideInInspector] public bool spawningEnemies = false;
	[SerializeField] int numEnemiesSpawned;
	[SerializeField] Transform enemySpawnPoint;
    [HideInInspector] GameObject enemySpawnerPlaceholder = null;//this will be filled in when it gets created
	//include which enemies will spawn 

	[Header("Fire Breath")]
	[SerializeField] float fireBreathDamage;
	[SerializeField] Transform fireBreathSpawnPoint;

	[Header("Ranged Attack")]
	[SerializeField] float rangedAttackDamage;
    [SerializeField] Transform rangedAttackSpawnPoint;

	[Header("Healthbar")]
	[SerializeField] GameObject ArmorBarCanvas;
	[SerializeField] Transform ArmorBar;
	[SerializeField] GameObject HealthBarCanvas;
	[SerializeField] Transform HealthBar;

    [Header("Stun Objects")]
	[SerializeField] GameObject[] armorObjects;
	[SerializeField] GameObject pinwheelObjectLeft;
	[SerializeField] GameObject pinwheelObjectRight;
	[SerializeField] ParticleSystem destroyParticles;
	private bool armorBroken = false;
	private bool armorRecentlyBroken = false;

	[Header("Game End Stuff")]
	[SerializeField] GameObject endGameObject;

	[Header("Sounds")]
	[SerializeField] FMODUnity.EventReference armorBreakingSFX;
	[SerializeField] FMODUnity.EventReference armorBlockingSFX;
	
	bool isSuspended = false;

	float bossFallRate = 2000;

	public Vector3 spawnPoint;
	public float movementRadius = 100f;

	[SerializeField] EnemyAttack stompAtk;


	// Start is called before the first frame update
	override protected void Start()
	{
		base.Start();

		isBoss = true;

		spawnPoint = transform.position;

		ArmorBarCanvas.SetActive(true);

		float armorBarScale = (Health / MaxHealth);
		HealthBar.localScale = new Vector3(armorBarScale * -1, 1, 1);

		GetAnimator().Play("BBYGH_Reveal_01 1");
	}

	private void FixedUpdate()
    {
		rb.AddForce(Vector3.down * bossFallRate);

		//base the armor on bool so it can be easliy turned on and off
		if(armorBroken == false)
        {
			foreach(GameObject armor in armorObjects)
            {
				armor.SetActive(true);
            }
			ArmorBarCanvas.SetActive(true);
		}

		if(GetAnimator().GetBool("isMoving"))
        {
			GetAnimator().Play("Baba_Yagas_House_Move");
			//GetAgent().velocity = -GetAgent().velocity;
			//RunAttack(stompAtk);
		}


		CheckPinwheels();
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
			//StartCoroutine(DelayMovement());
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
			//activeAttack.SpawnObject(enemySpawnPoint.position, enemySpawnPoint.rotation);
			GameObject enemySpawner = Instantiate(activeAttack.hitboxPrefab, enemySpawnPoint.position, enemySpawnPoint.rotation);
			enemySpawner.transform.SetParent(transform);
			enemySpawner.transform.SetAsLastSibling();
			enemiesSpawned = true;
			spawningEnemies = true;//check to make sure it doesnt take damage while in this state
			enemySpawnerPlaceholder = enemySpawner;

			armorBroken = false;
		}

	}

	public void NotSpawningEnemies()//this will get called by animation so it can be damaged again
    {
		spawningEnemies = false;
    }

	public void NotAttacking()
    {
		//GetAnimator().SetBool("isAttacking", false);
    }

	public GameObject getEnemySpawner()
    {
		return enemySpawnerPlaceholder;
    }
	public void BreatheFire()
    {
		if (activeAttack != null)
		{
			activeAttack.SpawnObject(fireBreathSpawnPoint.position, fireBreathSpawnPoint.rotation);
			activeAttack.damage = fireBreathDamage;
			//StartCoroutine(DelayMovement());
		}
	}

	public void RangedAttack()
    {
		if (activeAttack != null)
		{
			activeAttack.SpawnObject(rangedAttackSpawnPoint.position, rangedAttackSpawnPoint.rotation);
			activeAttack.damage = rangedAttackDamage;
			//StartCoroutine(DelayMovement());
		}

	}


    public override void TakeDamage(Attack atk, Vector3 attackForward, float damageAmp = 1, float knockbackMultiplier = 1)
    {
		if (armorBroken == true && !spawningEnemies && armorRecentlyBroken == false)
		{
			base.TakeDamage(atk, attackForward, damageAmp, 0.0f);//no knockback
			if (Health != MaxHealth || Health >= executionHealthThreshhold)
			{
				HealthBarCanvas.SetActive(true);
				float healthbarScale = (Health / MaxHealth);
				HealthBar.localScale = new Vector3(healthbarScale * -1, 1, 1);
			}
			else
				HealthBarCanvas.SetActive(false);
		}
		else if (atk.name == "Ram_Attack_Charge" && armorBroken == false)
		{
			ArmorBarCanvas.SetActive(false);
			HealthBarCanvas.SetActive(true);
			foreach (GameObject armor in armorObjects)
			{
				armor.SetActive(false);
			}
			destroyParticles.Play(true);
			armorBroken = true;
			GetAnimator().Play("BBYGH_Stun 1");
			//I-Frames
			StartCoroutine(ShieldRecentlyBroken());

			FMODUnity.RuntimeManager.PlayOneShot(armorBreakingSFX,transform.position);
		}
		//add a section for attacks that dont break shield for the sound 

	}

	public IEnumerator ShieldRecentlyBroken()
	{
		armorRecentlyBroken = true;
		yield return new WaitForSeconds(0.5f);
		armorRecentlyBroken = false;
	}

	public void StopMovement()
    {
		GetAgent().SetDestination(transform.position);
	}

	public override bool SetDestination(Vector3 dest)
    {
		GetAgent().enabled = true;//dont know why I have to do this, but for some reason, attacking the boss turns off the agent
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
			Debug.Log(dest);
			//GetAnimator().Play("Baba_Yagas_House_Move");
			if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5, 1) || GetAgent().isOnOffMeshLink)
			{
				transform.position = hit.position;
			}
			else
			{
				Debug.Log(gameObject + " tried finding a destination while not on a valid point");
				//base.OnDeath();
				return false;
			}
		}

		//StartCoroutine(DelayMovement());
		//GetAnimator().Play("Boss_Idle");//return to idle once movement is done
		return true;
	}
	
	//IEnumerator DelayMovement()
 //   {
	//	yield return new WaitForSeconds(0);
	//	MoveBoss();
	//	yield return new WaitForSeconds(0);

	//}

	//public void MoveBoss()
 //   {
	//	//for now, a predetermined vector
	//	Vector3 moveToPos = RandomPointInCircle(spawnPoint, 100f);

	//	SetDestination(moveToPos);

 //   }

	public void CheckPinwheels()//if they are spinning, boss gets stunned
    {
		if (pinwheelObjectLeft.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Pinwheel_Spin"))
		{
			GetAnimator().Play("BBYGH_Stun1");
		}

		if (pinwheelObjectRight.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Pinwheel_Spin"))
		{
			GetAnimator().Play("BBYGH_Stun1");
		}
	}

	Vector3 RandomPointInCircle(Vector3 center, float radius)
    {
		Vector3 randomPosition = UnityEngine.Random.insideUnitCircle * radius;
		Vector3 destinationPosition = new Vector3(randomPosition.x + center.x, center.y, randomPosition.z + center.z);

		Vector3 result = destinationPosition;
		//calculate
		//Debug.Log(result);

		return result;

	}
}