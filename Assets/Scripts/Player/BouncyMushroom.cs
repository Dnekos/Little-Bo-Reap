using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncyMushroom : MonoBehaviour
{
    [SerializeField] float bounceForce = 2000f;
    [SerializeField] float bounceCooldown = 1f;
    bool canBounce = true;

    private void OnTriggerEnter(Collider other)
    {
        //add upwards force
        if(canBounce && other.CompareTag("Player") && other.GetComponent<Rigidbody>() != null)
        {
            other.GetComponent<Rigidbody>().velocity = Vector3.zero;
            other.GetComponent<Rigidbody>().AddForce(this.transform.up * bounceForce);
            StartCoroutine(BounceCooldown());
        }
    }

    IEnumerator BounceCooldown ()
    {
        canBounce = false;
        yield return new WaitForSeconds(bounceCooldown);
        canBounce = true;
    }

}
