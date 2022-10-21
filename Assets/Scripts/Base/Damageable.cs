using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Damageable : MonoBehaviour
{
	[Header("Health")]
	[SerializeField] protected float Health;
	[SerializeField] protected float MaxHealth;
	public GameObject gibs;
	[SerializeField] GameObject damageNumber;
	[SerializeField] GameObject damageNumberGoth;
	public bool isInvulnerable;

	[Header("Drop Variables")]
	[Tooltip("Number of souls dropped on death.")]
	[SerializeField] protected int soulValue;
	[Tooltip("Affects speed at which souls fly out on death.")]
	[SerializeField] protected float soulSpeed;
	[Tooltip("Affects the height at which souls spawn from. should be higher for larger enemies.")]
	[SerializeField] protected float soulSpawnHeight;

	[Tooltip("The collectable that is worth one soul.")]
	[SerializeField] protected GameObject soulCollectableOne;

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
		SoulDropCalculation(soulValue);
		Instantiate(gibs, transform.position + Vector3.up * 1.4f, new Quaternion()).GetComponent<ParticleSystem>();
		Destroy(gameObject); // base effect is deleting object
	}
	//put it here because it wasn't overriding onDeath for non-executable enemies, likely because death was called in this function specifically
	private void SoulDropCalculation(int soulsToDrop)
	{
		while (soulsToDrop > 0)
		{
			var soulSpawnOffset = new Vector3(Random.Range(-1,1),soulSpawnHeight, Random.Range(-1,1));
			var soulCollectable = Instantiate(soulCollectableOne, transform.position + soulSpawnOffset, transform.rotation) as GameObject;
			soulCollectable.GetComponent<Rigidbody>().velocity = new Vector3(Random.Range(-1,1), 1f, Random.Range(-1,1)) * soulSpeed;
			Debug.Log("SoulDropped");
			soulsToDrop--;
		}
	}
}
