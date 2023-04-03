using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncyMushroom : MonoBehaviour
{
    [SerializeField] float bounceForce = 2000f;
    [SerializeField] float bounceCooldown = 1f;
    [SerializeField] string bounceAnim;
    [SerializeField] ParticleSystem bounceParticles;
    bool canBounce = true;
    [SerializeField] FMODUnity.EventReference bounceSFX;

    private void OnTriggerEnter(Collider other)
    {
        //add upwards force
        if(canBounce && other.CompareTag("Player") && other.GetComponent<Rigidbody>() != null)
        {
            //effects!
            GetComponent<Animator>().Play(bounceAnim);
            bounceParticles.Play(true);
            FMODUnity.RuntimeManager.PlayOneShot(bounceSFX,other.transform.position);

            //zero out velocity, then add force!
            //velocity must be zeroed out otherwise you might not get any movement if your already coming down fast
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
