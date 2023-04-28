using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    Vector3 origPos;

	
	[SerializeField]public EnemySpawn[] Enemies;
	

	[System.Serializable]
	public struct EnemySpawn
	{
		public GameObject EnemyPrefab;
		public Transform SpawnPoint; // can be spawned at point or vector, point has priority
		public Vector3 AlternateSpawn;
		[Range(1, 10)]
		public int NumEnemies;
		public float RandomRadius;
	}

	Transform SpawnedEnemiesFolder;

	// Start is called before the first frame update
	void Start()
    {
        origPos = transform.position;

		SpawnedEnemiesFolder = transform.GetChild(1);//this should be 'enemies'

		foreach (EnemySpawn enemy in Enemies)
		{
			for (int i = 0; i < enemy.NumEnemies; i++)
			{
				Vector3 SpawnPoint = (enemy.SpawnPoint == null) ? enemy.AlternateSpawn : enemy.SpawnPoint.position;
				SpawnPoint = SpawnPoint + new Vector3(Random.Range(-enemy.RandomRadius, enemy.RandomRadius), 0, Random.Range(-enemy.RandomRadius, enemy.RandomRadius));
				StartCoroutine(SpawnEnemy(enemy.EnemyPrefab, enemy.EnemyPrefab.GetComponent<EnemyAI>().SpawnParticlePrefab, SpawnPoint));
			}
		}

	}

	// Update is called once per frame
	void Update()
    {
        if(SpawnedEnemiesFolder.childCount == 0)
        {
			Destroy(gameObject);
			transform.parent.GetComponent<BabaYagasHouseAI>().NotSpawningEnemies();
			transform.parent.GetComponent<BabaYagasHouseAI>().GetAnimator().Play("BBYGH_Perch_Idle_Getup 1");

		}
	}

	protected IEnumerator SpawnEnemy(GameObject enemy, GameObject particle, Vector3 pos)
	{
		GameObject spawnPart = Instantiate(particle, pos, SpawnedEnemiesFolder.rotation, SpawnedEnemiesFolder);
		yield return new WaitForSeconds(enemy.GetComponent<EnemyAI>().SpawnWaitTime);
		Instantiate(enemy, pos, SpawnedEnemiesFolder.rotation, SpawnedEnemiesFolder);
		Destroy(spawnPart);
	}

}
