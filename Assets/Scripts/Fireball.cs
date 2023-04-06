using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour
{
	[SerializeField]
	Attack atk;

	[SerializeField]
	float force = 20;

	[SerializeField]
	GameObject Explosion;

    // Start is called before the first frame update
    void Start()
    {
		Transform target = GameObject.FindGameObjectWithTag("Player").transform;
		GetComponent<Rigidbody>().AddRelativeForce( (target.position - transform.position).normalized * force, ForceMode.Impulse);
		Destroy(gameObject, 3);
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
