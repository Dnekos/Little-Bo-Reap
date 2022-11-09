using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Collectable : MonoBehaviour
{
    bool isCollected;
    bool isPulled;
    [SerializeField] float attractSpeed;
    [SerializeField] float collectDistance = 1.5f;
    [SerializeField] float attractSpeedIncreaseOverTime = 1f;
	[HideInInspector]
	public GameObject playerBody;

    // Start is called before the first frame update
    void Start()
    {
        playerBody = GameManager.Instance.GetPlayer().gameObject;
    }

    // Update is called once per frame
    virtual protected void OnTriggerEnter(Collider col)
    {
        if ((col.CompareTag("Player") || col.CompareTag("GrabRadius")) && isCollected == false)
        {
            if (playerBody == null)
				playerBody = GameManager.Instance.GetPlayer().gameObject;
            isPulled = true;
            gameObject.layer = LayerMask.NameToLayer("Collectables");
            Debug.Log("GrabRadiusTriggered");
        }
        
    }

    void Update()
    {
        if (isPulled == true)
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
                Destroy(gameObject); //deletes self after being collected by default
            }
        }
    }

	protected abstract void CollectableEffect(); // put whatever you want the collectable to do in the override of this script
}
