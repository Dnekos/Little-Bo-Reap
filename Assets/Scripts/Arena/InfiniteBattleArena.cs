using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteBattleArena : BattleArena
{
	override protected void IncrementWaveNumber()
	{
		CurrentWave = (CurrentWave + 1) % waves.Length;
		Debug.Log("Next Wave: " + CurrentWave);
	}
}
