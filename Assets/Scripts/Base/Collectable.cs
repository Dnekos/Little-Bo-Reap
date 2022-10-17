using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    bool isCollected;
    bool isPulled;
    [SerializeField] float attractSpeed;
    GameObject playerBody;

    // Start is called before the first frame update
    void Start()
    {
        playerBody = GameObject.FindGameObjectWithTag("Player");

        //Ignore collisions with Enemy and Sheep Layers
        Physics.IgnoreLayerCollision(17, 10, true); //collectable and sheep layer
        Physics.IgnoreLayerCollision(17, 12, true); //collectable and enemy layer
        Physics.IgnoreLayerCollision(17, 13, true); //collectable and enemyAttack layer
        Physics.IgnoreLayerCollision(17, 14, true); //collectable and enemyExecute layer
    }

    // Update is called once per frame
    virtual protected void OnTriggerEnter(Collider col)
    {
        if (col.tag == "GrabRadius" && isCollected == false)
        {
            isPulled = true;
            Debug.Log("GrabRadiusTriggered");
        }
        if (col.tag == "Player" && isCollected == false)
        {
            isPulled = false;
            isCollected = true;
            Debug.Log("Colliding with player");
            CollectableEffect();
            Destroy(gameObject); //deletes self after being collected by default
        }
        
    }

    void Update()
    {
        if (isPulled == true)
        {
            var step = attractSpeed * Time.deltaTime; // calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, playerBody.transform.position, step);
        }
    }

    virtual protected void CollectableEffect()
    {
        //put whatever you want the collectable to do in the override of this script
    }
}
