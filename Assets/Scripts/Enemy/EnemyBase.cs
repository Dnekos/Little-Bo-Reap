using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : Damageable
{
    //This can be expanded, but for now it just needs spawning info for the battle arena script
    [Header("Spawning")]
    public GameObject SpawnParticlePrefab;
    public float SpawnWaitTime = 2;
    public float minSpawnStagger = 0f;
    public float maxSpawnStagger = 1f;

    //special enemies are most likely to be unsuckable so it goes here
    public bool suckResistant = false;
    float resistantTime = 0;
    public void SuckResistTimer(float duration)
    {
        suckResistant = true;
        resistantTime = duration;
        StartCoroutine("ResetSuckResist");
    }

    IEnumerator ResetSuckResist()
    {
        yield return new WaitForSeconds(resistantTime);
        suckResistant = false;
    }


}
