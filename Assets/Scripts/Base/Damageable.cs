using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Damageable : MonoBehaviour
{
	[Header("Health")]
	[SerializeField] protected float Health;
	[SerializeField] float MaxHealth;
	public GameObject gibs;
	[SerializeField] GameObject damageNumber;
	[SerializeField] GameObject damageNumberGoth;
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

			//create damage number
			var number = Instantiate(damageNumber, transform.position, transform.rotation) as GameObject;
			number.GetComponentInChildren<TextMeshProUGUI>().text = ((int)atk.damage).ToString();

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

			//create damage number
			var number = Instantiate(damageNumberGoth, transform.position, transform.rotation) as GameObject;
			number.GetComponentInChildren<TextMeshProUGUI>().text = ((int)atk.damageBlack).ToString();

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
