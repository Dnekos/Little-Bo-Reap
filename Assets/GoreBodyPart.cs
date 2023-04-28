using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoreBodyPart : MonoBehaviour
{
    [SerializeField] GameObject goreFx;
    [SerializeField] float lifeTime;
    [SerializeField] float maxRandTorque;
    void Start()
    {
        float torque = Random.Range(0, maxRandTorque);
        GetComponent<Rigidbody>().AddTorque(new Vector3(torque, torque, torque));

        Invoke("Chunks", lifeTime);
    }

    void Chunks()
    {
        WorldState.instance.pools.DequeuePooledObject(goreFx, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
