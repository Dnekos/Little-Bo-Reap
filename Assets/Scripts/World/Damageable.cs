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

	[Header("Sounds")]
	[SerializeField] protected FMODUnity.EventReference hurtSound;
	[SerializeField] protected FMODUnity.EventReference deathSound;


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
		if(!isInvulnerable || Health <= 0)
        {
			// deal damage
			Health -= atk.damage;
			Vector3 knockbackForce = attackForward * atk.forwardKnockback + Vector3.up * atk.upwardKnockback;

			Debug.Log(gameObject.name + " took " + atk.damage + " damage (force: "+ knockbackForce + ", mag "+ knockbackForce .magnitude+ ")");

			if (atk.ShowNumber)
			{
				//create damage number
				var number = Instantiate(damageNumber, transform.position, transform.rotation) as GameObject;
				number.GetComponentInChildren<TextMeshProUGUI>().text = ((int)atk.damage).ToString();
			}

			// add knockback if the current knockback is stronger than the current velocity
			//Vector3 knockbackForce = attackForward * atk.forwardKnockback + Vector3.up * atk.upwardKnockback;
			// this isnt working, maybe because the object is getting too much knockback in one frame?
			if (rb.velocity.sqrMagnitude < knockbackForce.sqrMagnitude)
			// TODO: make this not shit
			//if (true)
			{
				// we want to cancel out the current velocity, so as to knock launch them into the stratosphere
				//rb.AddForce(-rb.velocity, ForceMode.VelocityChange);
				rb.velocity = Vector3.zero;
				//Debug.Log("before vel: " + rb.velocity + " " + rb.velocity.magnitude);
				// TODO: make this force, not impulse, idiot
				rb.velocity = attackForward * atk.forwardKnockback + Vector3.up * atk.upwardKnockback;
				//Debug.Log("after vel: " + rb.velocity + " " + rb.velocity.magnitude);
			}

			// invoke death
			if (Health <= 0)
				OnDeath();
			else // don't play hurt sound when dying smh
				FMODUnity.RuntimeManager.PlayOneShotAttached(hurtSound, gameObject);

		}

	}

	virtual public void TakeDamage(SheepAttack atk, Vector3 attackForward)
    {
		if (!isInvulnerable)
		{
			// deal damage
			Health -= atk.damageBlack;
			Debug.Log(gameObject.name + " took " + atk.damageBlack + " damage (force: " + (attackForward * atk.forwardKnockbackBlack + Vector3.up * atk.upwardKnockbackBlack) + ")");

			if (atk.ShowNumber)
			{
				//create damage number
				var number = Instantiate(damageNumberGoth, transform.position, transform.rotation) as GameObject;
				number.GetComponentInChildren<TextMeshProUGUI>().text = ((int)atk.damageBlack).ToString();
			}

			// add knockback
			rb.AddForce(attackForward * atk.forwardKnockbackBlack + Vector3.up * atk.upwardKnockbackBlack, ForceMode.Impulse);

			// invoke death
			if (Health <= 0)
				OnDeath();
		}
	}
	
	virtual public void ForceKill()
    {
		OnDeath();
    }

	virtual protected void OnDeath()
	{
		SoulDropCalculation(soulValue);

		FMODUnity.RuntimeManager.PlayOneShot(deathSound, transform.position);

		Instantiate(gibs, transform.position + Vector3.up * 1.4f, new Quaternion());
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
