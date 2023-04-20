using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BBYGHArenaMeshManager : MonoBehaviour
{
    [SerializeField] GameObject EnemyWaveChild;
    [SerializeField] GameObject BBYGHMesh;

    // Update is called once per frame
    void Update()
    {
        if(EnemyWaveChild.transform.childCount >= 2)
        {
            Destroy(BBYGHMesh);
        }
    }
}
