using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterSpawn : MonoBehaviour
{
    [SerializeField] float time;
    // Start is called before the first frame update
    void Start()
    {
        //destroy this object immediately after spawning
        Destroy(gameObject, time);
    }

    
}
