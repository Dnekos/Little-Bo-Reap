using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BattleArena : PuzzleDoor
{
	[System.Serializable]
	public struct EnemyWave
	{
		public EnemySpawn[] Enemies;
	}

	[System.Serializable]
	public struct EnemySpawn
	{
		public GameObject EnemyPrefab;
		public Transform SpawnPoint; // can be spawned at point or vector, point has priority
		public Vector3 AlternateSpawn;
		[Range(1, 10)]
		public int NumEnemies;
		public float  RandomRadius;
	}

	[SerializeField]
	protected EnemyWave[] waves;
	protected int CurrentWave = -1;

	Transform SpawnedEnemiesFolder;

	[SerializeField] EnemyFlightPath[] FlightPaths;
	[SerializeField] float flightPathMaxDisplacement;

	[SerializeField] float slowTimeScale = 0.3f;
	[SerializeField] float slowTimeAtEnd = 1f;
	[SerializeField] GameObject slowTimeVolume;
	[SerializeField] GameObject colliderMesh;

	//soul spawning variables - might make this a struct later but it's only 2 varibles so it could be unnecessary.
	[SerializeField] Transform SoulSpawnPoint;
	[SerializeField] GameObject SoulReward;
	[SerializeField] GameObject spawnDelayParticle;

	[Header("Resetting"), SerializeField]
	GameEvent RespawnPlayer;

	[Header("Boot Player")]
	[SerializeField] Transform bootSpawnPoint;

	[Header("Music")]
	[SerializeField] int afterMusic;

	[Header("End Effects")]
	[SerializeField] GameObject finalCamera;
	[SerializeField] Vector3 yOffset;
	[SerializeField] Transform lookPoint;
	[SerializeField] float camSpawnSphereRadius = 5f;
	Vector3 finalEnemyPosition = Vector3.zero;
	bool finalEnemyConfirmed;



	// Start is called before the first frame update
	void Awake()
	{
		SpawnedEnemiesFolder = transform.GetChild(1);
		door.SetActive(false); // keep doors open

		if (colliderMesh != null && colliderMesh.GetComponent<MeshRenderer>() != null)
			colliderMesh.GetComponent<MeshRenderer>().enabled = false;

		RespawnPlayer?.Add(delegate { ResetArena(); });
	}

	// Update is called once per frame
	override protected void Update()
	{
		if (SpawnedEnemiesFolder.childCount == 0 && CurrentWave >= 0 && CurrentWave < waves.Length) // if killed all enemies AND waves started
			AdvanceWave(); // advance wave

		//if in the final wave, watch for the last enemy!
		if (CurrentWave == waves.Length - 1)
		{
			if (!finalEnemyConfirmed && SpawnedEnemiesFolder.childCount == 1)
				finalEnemyConfirmed = true;
			else
				finalEnemyPosition = SpawnedEnemiesFolder.GetChild(0).transform.position;
		}
	}

	void ResetArena()
	{
		if (!isOpened) // if arena is not cleared already
		{
			WorldState.instance.isInCombat = false;
			door.SetActive(false); // reopen doors
			CurrentWave = -1; // reset waves

			foreach (Transform child in SpawnedEnemiesFolder) // clear enemies
				Destroy(child.gameObject);
		}

	}

	virtual protected void IncrementWaveNumber()
	{
		CurrentWave++;
	}

	virtual protected void AdvanceWave()
	{
		IncrementWaveNumber();
		if (CurrentWave == waves.Length)
		{
			WorldState.instance.isInCombat = false;

			// if all waves done,
			OpenDoor();
			//DoorsFolder.SetActive(false); // open doors
			WorldState.instance.ChangeMusic(afterMusic);
			WorldState.instance.currentWorldTheme = afterMusic;
			Instantiate(SoulReward, SoulSpawnPoint.position, SoulSpawnPoint.rotation, SpawnedEnemiesFolder); //spawn soul reward

			//REVIEW: Looks good! Clear and efficient
			var cam = Instantiate(finalCamera, finalEnemyPosition + yOffset, Quaternion.identity) as GameObject;
			lookPoint.position = finalEnemyPosition;
			cam.GetComponent<ArenaEndCamera>().InitCamera(lookPoint, finalEnemyPosition);
			
			StartCoroutine(EndBattleSlow());
		}
		else
			// spawn each enemy
			foreach (EnemySpawn enemy in waves[CurrentWave].Enemies)
			{
				EnemyBase currEnemyPrefab = enemy.EnemyPrefab.GetComponent<EnemyBase>();
				for (int i = 0; i < enemy.NumEnemies; i++)
				{
					// get point from struct
					Vector3 SpawnPoint = (enemy.SpawnPoint == null) ? enemy.AlternateSpawn : enemy.SpawnPoint.position;

					// get stagger time
					float stagger = Random.Range(currEnemyPrefab.minSpawnStagger, currEnemyPrefab.maxSpawnStagger);

					SpawnPoint = (currEnemyPrefab is FlyingEnemyAI) ? SpawnPoint : FindSpawnPoint(enemy, SpawnPoint);

					// if we have a point, spawn the enemy
					if (SpawnPoint != Vector3.negativeInfinity)
						StartCoroutine(SpawnEnemy(enemy.EnemyPrefab, currEnemyPrefab.SpawnParticlePrefab, SpawnPoint, stagger));
				}
			}
	}

	Vector3 FindSpawnPoint(EnemySpawn enemy, Vector3 baseSpawnPoint)
	{
		NavMeshHit hit;

		Vector3 SpawnPoint;
		float redundancy = 20;
		// do checks to find a valid spawnpoint
		do
		{
			Vector2 rand = Random.insideUnitCircle;
			SpawnPoint = baseSpawnPoint + new Vector3(rand.x * enemy.RandomRadius, 0, rand.y * enemy.RandomRadius);

		} while (!NavMesh.SamplePosition(SpawnPoint, out hit, 5, NavMesh.AllAreas) && --redundancy > 0);

		// if we found a point, set the spawnpoint
		if (hit.hit)
			return hit.position;
		else
		{
			Debug.LogError("Could not find a valid point to spawn " + enemy.EnemyPrefab.name + " at " + baseSpawnPoint);
			return Vector3.negativeInfinity;
		}
	}

	public override void OpenDoor()
    {
		//for now, destroy door. can have an animation or something more pretty later
		isOpened = true;
		WorldState.instance.AddActivatedDoor(this);
		door.SetActive(false);
	}


    protected IEnumerator SpawnEnemy(GameObject enemy, GameObject particle, Vector3 pos, float staggerDelay)
	{


		//delay
		var staggerParticle = Instantiate(spawnDelayParticle, pos, SpawnedEnemiesFolder.rotation, SpawnedEnemiesFolder) as GameObject;
		var module = staggerParticle.GetComponent<ParticleSystem>().main;
		module.duration = staggerDelay + 1;
		module.startLifetime = staggerDelay;
		staggerParticle.GetComponent<ParticleSystem>().Play(true);

		yield return new WaitForSeconds(staggerDelay);

		Instantiate(particle, pos, SpawnedEnemiesFolder.rotation, SpawnedEnemiesFolder);
		yield return new WaitForSeconds(enemy.GetComponent<EnemyBase>().SpawnWaitTime);
		//Instantiate(enemy, pos, SpawnedEnemiesFolder.rotation, SpawnedEnemiesFolder).GetComponent<EnemyAI>().ToChase();
		//I see "ToChase()" is just an empty function so I commented it out

		
		GameObject newEnemy = Instantiate(enemy, pos, SpawnedEnemiesFolder.rotation, SpawnedEnemiesFolder);

		//if the enemy has a spline follower script(that means it is a flying enemy)
		//then find the index the flying enemy has and attach it to the corresponding flight path in this script's array
		AttachToSpline(newEnemy);
    }

	private void AttachToSpline(GameObject enemy)
	{
		if(enemy.GetComponent<SplineFollower>() != null)
		{
			enemy.GetComponent<SplineFollower>().path = FlightPaths[enemy.GetComponent<FlyingEnemyAI>().flightPathIndex];

			//add a random position offset. without this, they all stack up on top of each other
			float rand = Random.Range(0f, flightPathMaxDisplacement);
			enemy.GetComponent<SplineFollower>().SplinePosition += rand;
		}
	}

    private void OnTriggerEnter(Collider other)
	{
		if (!isOpened && CurrentWave == -1 && other.gameObject.CompareTag("Player")) // untriggered
		{
			door.SetActive(true);
			AdvanceWave();
			WorldState.instance.ChangeMusic(1);
			WorldState.instance.InitCombatBootPoint(bootSpawnPoint);
		}
	}

	IEnumerator EndBattleSlow()
    {
		slowTimeVolume.SetActive(true);
		Time.timeScale = slowTimeScale;
		Time.fixedDeltaTime = 0.02F * Time.timeScale;

		yield return new WaitForSeconds(slowTimeAtEnd);

		slowTimeVolume.SetActive(false);
		Time.timeScale = 1f;
		Time.fixedDeltaTime = 0.02F * Time.timeScale;
		
	}
}
