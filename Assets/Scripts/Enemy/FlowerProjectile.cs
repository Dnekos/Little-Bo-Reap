using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerProjectile : MonoBehaviour
{
    [SerializeField] Attack FlowerProjectileAttack;
	[SerializeField] float projectileInitialForce =2000f;
	[SerializeField] ForceMode launchType = ForceMode.Force;
    List<Damageable> hitTargets;

    Vector3 origPos;

    [SerializeField] float maxTimeAlive = 2.5f;
    float currentTimeAlive = 0;

    // Start is called before the first frame update
    void Start()
    {
        hitTargets = new List<Damageable>();

        origPos = transform.position;

        FireProjectile();
		Destroy(gameObject, maxTimeAlive);
    }

    private void OnTriggerEnter(Collider other)
    {
		// im sorry what is this
        Vector3 flattenedOtherPos = new Vector3(other.transform.position.x, transform.position.y, other.transform.position.z);
        Damageable targetHealth = other.GetComponent<Damageable>();
		if (targetHealth != null && !other.isTrigger)
		{
			//hit target takes damage
			hitTargets.Add(targetHealth);
			Debug.Log(other.gameObject.name + "hit by Flower Projectile Attack");
			targetHealth.TakeDamage(FlowerProjectileAttack, (flattenedOtherPos - origPos).normalized);

			// kill yourself
			Destroy(gameObject);
		}
    }


    private void FireProjectile()
    {
        this.GetComponentInChildren<Rigidbody>().AddForce(this.transform.forward * 2000f, launchType);
    }

}
