using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
    bool isCollected;
    bool isPulled;
    [SerializeField] float attractSpeed;
    public GameObject playerBody;

    // Start is called before the first frame update
    void Start()
    {
        playerBody = GameManager.Instance.GetPlayer().gameObject;
    }

    // Update is called once per frame
    virtual protected void OnTriggerEnter(Collider col)
    {
        if (col.tag == "GrabRadius" && isCollected == false)
        {
            if (playerBody == null) playerBody = GameManager.Instance.GetPlayer().gameObject;
            isPulled = true;
            gameObject.layer = LayerMask.NameToLayer("Collectables");
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
            transform.position = Vector3.Lerp(transform.position, playerBody.transform.position, step);
        }
    }

    virtual protected void CollectableEffect()
    {
        //put whatever you want the collectable to do in the override of this script
    }
}
