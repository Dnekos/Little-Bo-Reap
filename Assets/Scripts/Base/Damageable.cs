using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{
	[Header("Health")]
	[SerializeField] float Health;
	[SerializeField] float MaxHealth;
	[SerializeField] GameObject gibs;
	public bool isInvulnerable; 

	protected Rigidbody rb;

    // Start is called before the first frame update
    virtual protected void Start()
    {
		Health = MaxHealth;
		rb = GetComponent<Rigidbody>();
    }

	virtual public void TakeDamage(Attack atk, Vector3 attackForward)
	{
		if(!isInvulnerable)
        {
			// deal damage
			Health -= atk.damage;
			Debug.Log(gameObject.name + " took " + atk.damage + " damage (force: "+(attackForward * atk.forwardKnockback + Vector3.up * atk.upwardKnockback)+")");

			// add knockback
			rb.AddForce(attackForward * atk.forwardKnockback + Vector3.up * atk.upwardKnockback, ForceMode.Impulse);

			// invoke death
			if (Health <= 0)
				OnDeath();
		}
		
	}
	virtual protected void OnDeath()
	{
		Instantiate(gibs, transform.position, transform.rotation);
		Destroy(gameObject); // base effect is deleting object
	}	
}
