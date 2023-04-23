using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BBYGHArenaMeshManager : MonoBehaviour
{
    [SerializeField] GameObject EnemyWaveChild;
    [SerializeField] GameObject BBYGHMesh;
	[SerializeField] GameEvent respawn;

	private void Start()
	{
		respawn.Add(ReEnable);
	}

	// Update is called once per frame
	void Update()
    {
        if(EnemyWaveChild.transform.childCount >= 2)
        {
            BBYGHMesh.SetActive(false);
        }
    }

	void ReEnable()
	{
		BBYGHMesh.SetActive(true);
	}
}
