using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : Damageable
{
    //This can be expanded, but for now it just needs spawning info for the battle arena script
    [Header("Spawning")]
    public GameObject SpawnParticlePrefab;
    public float SpawnWaitTime = 2;

}
