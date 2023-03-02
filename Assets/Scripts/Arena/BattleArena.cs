using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleArena : MonoBehaviour
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

	GameObject DoorsFolder;
	Transform SpawnedEnemiesFolder;

	[SerializeField] EnemyFlightPath[] FlightPaths;

	[SerializeField] float slowTimeScale = 0.3f;
	[SerializeField] float slowTimeAtEnd = 1f;
	[SerializeField] GameObject slowTimeVolume;
	[SerializeField] GameObject colliderMesh;

	//soul spawning variables - might make this a struct later but it's only 2 varibles so it could be unnecessary.
	[SerializeField] Transform SoulSpawnPoint;
	[SerializeField] GameObject SoulReward;

	[Header("Resetting"), SerializeField]
	GameEvent RespawnPlayer;

	// Start is called before the first frame update
	void Start()
	{
		SpawnedEnemiesFolder = transform.GetChild(1);
		DoorsFolder = transform.GetChild(2).gameObject;
		DoorsFolder.SetActive(false); // keep doors open
		colliderMesh.GetComponent<MeshRenderer>().enabled = false;

		RespawnPlayer?.listener.AddListener(delegate { ResetArena(); });
	}

	// Update is called once per frame
	void Update()
	{
		if (SpawnedEnemiesFolder.childCount == 0 && CurrentWave >= 0 && CurrentWave < waves.Length) // if killed all enemies AND waves started
			AdvanceWave(); // advance wave
	}

	void ResetArena()
	{
		if (CurrentWave != waves.Length) // if arena is not cleared already
		{
			DoorsFolder.SetActive(false); // reopen doors
			CurrentWave = -1; // reset waves

			foreach (Transform child in SpawnedEnemiesFolder) // clear enemies
				Destroy(child.gameObject);
		}

	}

	virtual protected void AdvanceWave()
	{
		CurrentWave++;
		if (CurrentWave == waves.Length)
		{
			// if all waves done,
			DoorsFolder.SetActive(false); // open doors
			Instantiate(SoulReward, SoulSpawnPoint.position, SoulSpawnPoint.rotation, SpawnedEnemiesFolder); //spawn soul reward
			StartCoroutine(EndBattleSlow());
		}
		else
			// spawn each enemy
			foreach (EnemySpawn enemy in waves[CurrentWave].Enemies)
			{
				for (int i = 0; i < enemy.NumEnemies; i++)
				{
					Vector3 SpawnPoint = (enemy.SpawnPoint == null) ? enemy.AlternateSpawn : enemy.SpawnPoint.position;
					SpawnPoint = SpawnPoint + new Vector3(Random.Range(-enemy.RandomRadius, enemy.RandomRadius), 0, Random.Range(-enemy.RandomRadius, enemy.RandomRadius));
					StartCoroutine(SpawnEnemy(enemy.EnemyPrefab, enemy.EnemyPrefab.GetComponent<EnemyBase>().SpawnParticlePrefab, SpawnPoint));

				}
			}
				

	}

	protected IEnumerator SpawnEnemy(GameObject enemy, GameObject particle, Vector3 pos)
	{

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
		}
	}

    private void OnTriggerEnter(Collider other)
	{
		if (CurrentWave == -1 && other.gameObject.CompareTag("Player")) // untriggered
		{
			DoorsFolder.SetActive(true);
			AdvanceWave();
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
