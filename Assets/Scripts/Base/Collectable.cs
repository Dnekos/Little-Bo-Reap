using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    bool isCollected;

    // Start is called before the first frame update
    virtual protected void Start()
    {
        
    }

    // Update is called once per frame
    virtual protected void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player" && isCollected == false)
        {
            isCollected = true;
            Debug.Log("Colliding with player");
            CollectableEffect();
            Destroy(gameObject); //deletes self after being collected by default
        }
        
    }

    virtual protected void CollectableEffect()
    {
        //put whatever you want the collectable to do in the override of this script
    }
}
