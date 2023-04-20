using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour
{
	[SerializeField]
	Attack atk;

	[SerializeField]
	float force = 20;

	[SerializeField] GameObject Explosion;
	[SerializeField] FMODUnity.EventReference whistle;

    // Start is called before the first frame update
    void Start()
    {
		Transform target = GameObject.FindGameObjectWithTag("Player").transform;
		GetComponent<Rigidbody>().AddRelativeForce( (target.position - transform.position).normalized * force, ForceMode.Impulse);
		Destroy(gameObject, 3);
		FMODUnity.RuntimeManager.PlayOneShot(whistle, transform.position);
    }
	private void OnCollisionEnter(Collision collision)
	{
		Damageable target = collision.gameObject.GetComponent<Damageable>();
		if (target != null)
		{
			target.TakeDamage(atk, collision.GetContact(0).normal);
		}
		Instantiate(Explosion, transform.position, transform.rotation).layer = gameObject.layer;
		Destroy(gameObject);
	}
}
