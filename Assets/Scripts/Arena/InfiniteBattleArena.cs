using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteBattleArena : BattleArena
{
	override protected void AdvanceWave()
	{
		CurrentWave = (CurrentWave + 1) % waves.Length;

		// spawn each enemy
		foreach (EnemySpawn enemy in waves[CurrentWave].Enemies)
		{
			Debug.Log(enemy.NumEnemies);
			for (int i = 0; i < enemy.NumEnemies; i++)
			{
				Vector3 SpawnPoint = (enemy.SpawnPoint == null) ? enemy.AlternateSpawn : enemy.SpawnPoint.position;
				SpawnPoint = SpawnPoint + new Vector3(Random.Range(-enemy.RandomRadius, enemy.RandomRadius), 0, Random.Range(-enemy.RandomRadius, enemy.RandomRadius));

				// particles if able
				EnemyAI ai = enemy.EnemyPrefab.GetComponent<EnemyAI>();
				if (ai != null)
					StartCoroutine(SpawnEnemy(enemy.EnemyPrefab, enemy.EnemyPrefab.GetComponent<EnemyAI>().SpawnParticlePrefab, SpawnPoint));

			}
		}
	}
}
