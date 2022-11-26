using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSheepProjectile : MonoBehaviour
{
	[Header("Sounds")]
	[SerializeField] FMODUnity.EventReference biteSound;
	[SerializeField] FMODUnity.EventReference launchSound;

	[Header("Launch Projectile Variables")]
    [SerializeField] float launchForce = 2500f;
    [SerializeField] float launchForceLift = 250f;
    [SerializeField] float lifeTime = 10f;
    [SerializeField] float lifeTimeAfterAttack = 1.5f;
    [SerializeField] SheepAttack launchAttack;
    [SerializeField] GameObject blackSheepParticles;
    [SerializeField] GameObject gibs;
    public bool isBlackSheep = false;

    Rigidbody rb;

    void Awake()
    {
        Invoke("DestroySheepProjectile", lifeTime);
        rb = GetComponent<Rigidbody>();
    }

    void DestroySheepProjectile()
    {
        Instantiate(gibs, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    private void Start()
    {
		// check black sheep stuff
		if (isBlackSheep)
			blackSheepParticles.SetActive(true);

		FMOD.Studio.EventInstance eventInst = FMODUnity.RuntimeManager.CreateInstance(launchSound);
		FMODUnity.RuntimeManager.AttachInstanceToGameObject(eventInst, this.transform, rb);
		eventInst.start();
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
	
	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.CompareTag("Enemy"))
		{
			FMODUnity.RuntimeManager.PlayOneShot(biteSound, transform.position);
			gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
			Vector3 forcePoint = new Vector3(collision.GetContact(0).normal.x, 0, collision.GetContact(0).normal.z);

			if (isBlackSheep)
			{
				collision.gameObject?.GetComponent<Damageable>()?.TakeDamage(launchAttack, -forcePoint);
				Instantiate(launchAttack.explosionEffect, transform.position, transform.rotation);
				DestroySheepProjectile();
			}
			else
			{
				collision.gameObject?.GetComponent<Damageable>()?.TakeDamage((Attack)launchAttack, -forcePoint);
				Invoke("DestroySheepProjectile", lifeTimeAfterAttack);
			}
		}
	}
}