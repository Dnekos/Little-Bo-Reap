using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSheepProjectile : MonoBehaviour
{
	[SerializeField] FMODUnity.EventReference launchSound;

	[Header("Launch Projectile Variables")]
    [SerializeField] float launchForce = 2500f;
    [SerializeField] float launchForceLift = 250f;
    [SerializeField] float lifeTime = 10f;
    [SerializeField] float lifeTimeAfterAttack = 1.5f;
    [SerializeField] SheepAttack launchAttack;
    [SerializeField] GameObject blackSheepParticles;
    public bool isBlackSheep = false;

    Rigidbody rb;

    void Awake()
    {
        Destroy(gameObject, lifeTime);
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
		// check black sheep stuff
		if (isBlackSheep)
			blackSheepParticles.SetActive(true);

		FMOD.Studio.EventInstance eventInst = FMODUnity.RuntimeManager.CreateInstance(launchSound);
		FMODUnity.RuntimeManager.AttachInstanceToGameObject(eventInst, this.transform, rb);
		eventInst.start();

		//FMODUnity.RuntimeManager.PlayOneShotAttached(launchSound, gameObject);

	}

	public void LaunchProjectile()
    {
        rb.AddForce(Camera.main.transform.forward * launchForce + transform.up * launchForceLift);
        rb.AddTorque(100f, 100f, 100f);
    }

    public void LaunchProjectile(Vector3 dir)
    {
        rb.AddForce(dir.normalized * launchForce + transform.up * launchForceLift);
        rb.AddTorque(100f, 100f, 100f);
    }

  // private void OnTriggerEnter(Collider other)
  // {
  //     if (other.CompareTag("Enemy"))
  //     {
  //         other?.GetComponent<EnemyAI>().TakeDamage(launchAttack, transform.forward);
  //         Destroy(gameObject, lifeTimeAfterAttack);
  //     }
  //
  // }
	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.CompareTag("Enemy"))
		{
            if(isBlackSheep)
            {
                gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                Vector3 forcePoint = new Vector3(collision.GetContact(0).normal.x, 0, collision.GetContact(0).normal.z);
                collision.gameObject?.GetComponent<EnemyAI>().TakeDamage(launchAttack, -forcePoint);
                Instantiate(launchAttack.explosionEffect, transform.position, transform.rotation);
                Destroy(gameObject, lifeTimeAfterAttack);
            }
            else
            {
                gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                Vector3 forcePoint = new Vector3(collision.GetContact(0).normal.x, 0, collision.GetContact(0).normal.z);
                collision.gameObject?.GetComponent<EnemyAI>().TakeDamage((Attack)launchAttack, -forcePoint);
                Destroy(gameObject, lifeTimeAfterAttack);
            }
		}
	}
}
