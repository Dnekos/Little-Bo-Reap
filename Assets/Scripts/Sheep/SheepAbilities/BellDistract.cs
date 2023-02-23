using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BellDistract : MonoBehaviour
{
    [SerializeField] float range;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Enemy")
        {
            other.GetComponent<EnemyAI>().distracted = true;
            other.GetComponent<EnemyAI>().bellLoc = this.transform.position;
        }
    }
}
