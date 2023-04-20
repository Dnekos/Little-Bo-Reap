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
	[SerializeField] GameObject sheepMesh;
    public bool isBlackSheep = false;
	public int SheepType;
    Rigidbody rb;

    void Awake()
    {
        Invoke("DestroySheepProjectile", lifeTime);
        rb = GetComponent<Rigidbody>();
    }

    void DestroySheepProjectile()
    {
		WorldState.instance.pools.DequeuePooledObject(gibs, transform.position, transform.rotation);
        //Instantiate();
        Destroy(gameObject);
    }

    private void Start()
    {
		// check black sheep stuff
		if (isBlackSheep)
			blackSheepParticles.SetActive(true);

		FMOD.Studio.EventInstance eventInst = FMODUnity.RuntimeManager.CreateInstance(launchSound);
		FMODUnity.RuntimeManager.AttachInstanceToGameObject(eventInst, this.transform, rb);
		if (SheepType == 0 && WorldState.instance.PersistentData.activeUpgrades.HasFlag(SaveData.Upgrades.BuilderLaunchDam))
			eventInst.setParameterByName("Progression", 1);
		eventInst.start();
		eventInst.release();

		eventInst.start();
	}

	public void SetMeshScale(float size)
    {
		sheepMesh.transform.localScale = new Vector3(size, size, size);
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
				if (SheepType == 0)
				{
					collision.gameObject?.GetComponent<Damageable>()?.TakeDamage(launchAttack.BSAttack, -forcePoint, WorldState.instance.passiveValues.builderLaunchDam, 1.0f);
				}
				if (SheepType == 1)
				{
					collision.gameObject?.GetComponent<Damageable>()?.TakeDamage(launchAttack.BSAttack, -forcePoint, WorldState.instance.passiveValues.ramDamage, WorldState.instance.passiveValues.ramKnockback);
				}
				else if (SheepType == 2) //checks for fluffy sheep by checking if it's not a ram OR a builder. Because we don't have an isFluffySheep. The fuck?
				{
					collision.gameObject?.GetComponent<Damageable>()?.TakeDamage(launchAttack.BSAttack, -forcePoint);
				}
				Instantiate(launchAttack.explosionEffect, transform.position, transform.rotation);
				DestroySheepProjectile();
			}
			else
			{
				if (SheepType == 0) 
				{ 
					collision.gameObject?.GetComponent<Damageable>()?.TakeDamage(launchAttack, -forcePoint, WorldState.instance.passiveValues.builderLaunchDam); 
				}
				if (SheepType == 1)
				{ 
					collision.gameObject?.GetComponent<Damageable>()?.TakeDamage(launchAttack, -forcePoint, WorldState.instance.passiveValues.ramDamage, WorldState.instance.passiveValues.ramKnockback); 
				}
				else if (SheepType == 2) //checks for fluffy sheep by checking if it's not a ram OR a builder. Because we don't have an isFluffySheep. The fuck?
                {
					collision.gameObject?.GetComponent<Damageable>()?.TakeDamage(launchAttack, -forcePoint);
				}
				Invoke("DestroySheepProjectile", lifeTimeAfterAttack);
			}
		}
		else if (collision.gameObject.CompareTag("Target"))
        {
			collision.gameObject.GetComponent<LaunchTarget>().OpenKey();
        }
	}
}
