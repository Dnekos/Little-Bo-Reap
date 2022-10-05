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

	virtual public void TakeDamage(SheepAttack atk, Vector3 attackForward)
    {
		if (!isInvulnerable)
		{
			// deal damage
			Health -= atk.damageBlack;
			Debug.Log(gameObject.name + " took " + atk.damageBlack + " damage (force: " + (attackForward * atk.forwardKnockbackBlack + Vector3.up * atk.upwardKnockbackBlack) + ")");

			// add knockback
			rb.AddForce(attackForward * atk.forwardKnockbackBlack + Vector3.up * atk.upwardKnockbackBlack, ForceMode.Impulse);

			// invoke death
			if (Health <= 0)
				OnDeath();
		}
	}
	

	virtual protected void OnDeath()
	{
		Instantiate(gibs, transform.position + Vector3.up * 1.4f, new Quaternion()).GetComponent<ParticleSystem>();
		Destroy(gameObject); // base effect is deleting object
	}	
}
