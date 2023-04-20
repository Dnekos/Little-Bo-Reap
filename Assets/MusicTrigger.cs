using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicTrigger : MonoBehaviour
{
    WorldState ws;
    private void Start()
    {
        ws = FindObjectOfType<WorldState>();
    }
    private void OnTriggerEnter(Collider other)
    {

        if(other.gameObject.CompareTag("Player"))
        {
            
            ws.ChangeMusic(6);
            Debug.Log("Swamp theme");
        }
    }
}
