using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoreBodyPartSpawner : MonoBehaviour
{
    [SerializeField] Transform spawnPoint;
    [SerializeField] GameObject goreObject;
    void Start()
    {
        Instantiate(goreObject, spawnPoint.position, spawnPoint.rotation);
    }

    
}
