using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncyMushroom : MonoBehaviour
{
    [SerializeField] float bounceForce = 2000f;

    private void OnTriggerEnter(Collider other)
    {
        //add upwards force
        other.GetComponent<Rigidbody>()?.AddForce(transform.up * bounceForce);
    }
}
