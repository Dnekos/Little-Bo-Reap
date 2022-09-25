using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damageable : MonoBehaviour
{
	[Header("Health")]
	[SerializeField] float Health;
	[SerializeField] float MaxHealth;

	protected Rigidbody rb;

    // Start is called before the first frame update
    virtual protected void Start()
    {
		Health = MaxHealth;
		rb = GetComponent<Rigidbody>();
    }

	virtual public void TakeDamage(Attack atk, Vector3 attackForward)
	{
		// deal damage
		Health -= atk.damage;
		Debug.Log(gameObject.name + " took " + atk.damage + " damage");

		// add knockback
		rb.AddForce(attackForward.x,
					Mathf.Abs(attackForward.y) * atk.upwardKnockback,
					attackForward.z * atk.forwardKnockback, ForceMode.Impulse);
		
		// invoke death
		if (Health <= 0) 
			OnDeath();
	}
	virtual protected void OnDeath()
	{
		Destroy(gameObject); // base effect is deleting object
	}	
}
