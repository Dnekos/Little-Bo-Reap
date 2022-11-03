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
	}

	[SerializeField]
	EnemyWave[] waves;
	int CurrentWave = -1;

	GameObject DoorsFolder;
	Transform SpawnedEnemiesFolder;

	[Header("Resetting"), SerializeField]
	GameEvent RespawnPlayer;

	// Start is called before the first frame update
	void Start()
	{
		SpawnedEnemiesFolder = transform.GetChild(1);
		DoorsFolder = transform.GetChild(2).gameObject;
		DoorsFolder.SetActive(false); // keep doors open

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

	void AdvanceWave()
	{
		CurrentWave++;
		if (CurrentWave == waves.Length) // if all waves done,
		{
			DoorsFolder.SetActive(false); // open doors
		}
		else
		{
			// spawn each enemy
			foreach (EnemySpawn enemy in waves[CurrentWave].Enemies)
			{
				StartCoroutine(SpawnEnemy(enemy.EnemyPrefab, enemy.EnemyPrefab.GetComponent<EnemyAI>().SpawnParticlePrefab, (enemy.SpawnPoint == null) ? enemy.AlternateSpawn : enemy.SpawnPoint.position));
			}
		}

	}

	IEnumerator SpawnEnemy(GameObject enemy, GameObject particle, Vector3 pos)
	{

		Instantiate(particle, pos, SpawnedEnemiesFolder.rotation, SpawnedEnemiesFolder);
		yield return new WaitForSeconds(enemy.GetComponent<EnemyAI>().SpawnWaitTime);
		Instantiate(enemy, pos, SpawnedEnemiesFolder.rotation, SpawnedEnemiesFolder).GetComponent<EnemyAI>().ToChase();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (CurrentWave == -1 && other.gameObject.CompareTag("Player")) // untriggered
		{
			DoorsFolder.SetActive(true);
			AdvanceWave();
		}
	}
}
