using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class Collectable : MonoBehaviour
{
    bool isCollected;
    bool isPulled;
    [SerializeField] float attractSpeed;
    [SerializeField] float collectDistance = 1.5f;
    [SerializeField] float attractSpeedIncreaseOverTime = 100f;
    [SerializeField] GameObject collectParticles;
	[HideInInspector]
	public GameObject playerBody;

    // Start is called before the first frame update
    void Start()
    {
        playerBody = WorldState.instance.player;
    }

    // Update is called once per frame
    virtual protected void OnTriggerEnter(Collider col)
    {
        if ((col.CompareTag("Player") || col.CompareTag("GrabRadius")) && isCollected == false)
        {
            if (playerBody == null)
				playerBody = WorldState.instance.player;
            isPulled = true;
            gameObject.layer = LayerMask.NameToLayer("Collectables");
            Debug.Log("GrabRadiusTriggered");
        }
        
    }

    protected void SpawnCollectParticles()
    {
        if(collectParticles != null)
        {
            Instantiate(collectParticles, transform.position, transform.rotation);
        }
    }

    void Update()
    {
        if (isPulled == true)//REVIEW: this can be made into its own function, rather than all the code be in the update function
                             //Also if need be, this code could then be accessed outside of this script
        {
            var step = attractSpeed * Time.deltaTime; // calculate distance to move
            transform.position = Vector3.Lerp(transform.position, playerBody.transform.position, step);

            attractSpeed +=  attractSpeedIncreaseOverTime * Time.deltaTime;

            if(isCollected == false && Vector3.Distance(playerBody.transform.position, transform.position) <= collectDistance)
            {
                isPulled = false;
                isCollected = true;
                Debug.Log("Colliding with player");
                CollectableEffect();
                if(collectParticles !=null) SpawnCollectParticles();
                Destroy(gameObject); //deletes self after being collected by default
            }
        }
    }

    private void FixedUpdate()//REVIEW: We can delete this to clean the script up a little
    {
      // if (isPulled == true)
      // {
      //     var dir = (playerBody.transform.position - transform.position).normalized;
      //     GetComponent<Rigidbody>().AddForce(dir * attractSpeedIncreaseOverTime);
      // }
    }

    protected abstract void CollectableEffect(); // put whatever you want the collectable to do in the override of this script
}
