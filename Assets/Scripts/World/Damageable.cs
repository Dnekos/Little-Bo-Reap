using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Damageable : MonoBehaviour
{
	//TODO figure out how to keep health protected
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
	[Tooltip("Number of healthPickups dropped on death.")]
	[SerializeField] protected int healthValue;
	[Tooltip("Affects speed at which souls fly out on death.")]
	[SerializeField] protected float soulSpeed;
	[Tooltip("Affects the height at which souls spawn from. should be higher for larger enemies.")]
	[SerializeField] protected float soulSpawnHeight;

	[Tooltip("The Array of Soul Collectable increments.")]
	[SerializeField] protected GameObject[] soulCollectables;

	protected Rigidbody rb;

	// Start is called before the first frame update
	virtual protected void Start()
	{
		Health = MaxHealth;
		rb = GetComponent<Rigidbody>();
	}
	//Non Sheep TakeDamage
	virtual public void TakeDamage(Attack atk, Vector3 attackForward, float damageAmp = 1, float knockbackMultiplier = 1)
	{
		if (!isInvulnerable || Health <= 0)
		{
			// deal damage
			Health -= atk.damage * ((damageAmp <= 0) ? 1 : damageAmp);

			// knockback
			DealKnockback(attackForward, atk.forwardKnockback, atk.upwardKnockback, knockbackMultiplier);
			
			//Debug.Log(gameObject.name + " took " + atk.damage * ((damageAmp <= 0) ? 1 : damageAmp) + " damage (force: " + knockbackForce + ", mag " + knockbackForce.magnitude + ")");
			//Debug.Log(gameObject.name + " took " + atk.damage + " damage (force: " + knockbackForce + ", mag " + knockbackForce.magnitude + ")");

			if (atk.ShowNumber)
			{
				if(atk is SheepAttack)
                {
					//create damage number
					GameObject number = Instantiate(damageNumber, transform.position, transform.rotation);
					number.GetComponentInChildren<TextMeshProUGUI>().text = ((int)atk.damage).ToString();
				}
				else
                {
					//create damage number
					GameObject number = Instantiate(damageNumberGoth, transform.position, transform.rotation);
					number.GetComponentInChildren<TextMeshProUGUI>().text = ((int)atk.damage).ToString();
				}

				
			}

			// invoke death
			if (Health <= 0)
				OnDeath();
			else // don't play hurt sound when dying smh
				FMODUnity.RuntimeManager.PlayOneShotAttached(hurtSound, gameObject);

		}
	}

	// TODO: Mitko fix this function 
	/// <summary>
	/// 
	/// </summary>
	/// <param name="forward">direction f_mag points in</param>
	/// <param name="f_mag">magnitude of forward</param>
	/// <param name="u_mag">magnitude of force in direction of world up</param>
	/// <param name="mult">overall multiplier of knockback</param>

	//------------------------------------------------------------------
	//old Knockback implementation(eliminated extreme knockback
	//void DealKnockback(Vector3 forward, float f_mag, float u_mag, float mult = 1)
	//{
	//	Vector3 knockbackForce = (forward * f_mag + Vector3.up * u_mag) * mult;


	//	// add knockback if the current knockback is stronger than the current velocity
	//	//Vector3 knockbackForce = attackForward * atk.forwardKnockback + Vector3.up * atk.upwardKnockback;
	//	// this isnt working, maybe because the object is getting too much knockback in one frame?
	//	if (rb.velocity.sqrMagnitude < knockbackForce.sqrMagnitude)
	//	// TODO: make this not shit
	//	//if (true)
	//	{
	//		// we want to cancel out the current velocity, so as to knock launch them into the stratosphere
	//		//rb.AddForce(-rb.velocity, ForceMode.VelocityChange);
	//		rb.velocity = Vector3.zero;
	//		Debug.Log("before vel: " + rb.velocity + " " + rb.velocity.magnitude);
	//		// TODO: make this force, not impulse, idiot
	//		rb.velocity = forward * f_mag + Vector3.up * u_mag;
	//		Debug.Log("after vel: " + rb.velocity + " " + rb.velocity.magnitude);
	//	}
	//}
	//------------------------------------------------------------------

	void DealKnockback(Vector3 forward, float f_mag, float u_mag, float mult = 1)
	{
		Vector3 knockbackForce = (forward * f_mag + Vector3.up * u_mag) * mult;

		// add knockback if the current knockback is stronger than the current velocity
		if (rb.velocity.sqrMagnitude < knockbackForce.sqrMagnitude)
		{
			//float knockbackMult = knockbackForce.magnitude - rb.velocity.magnitude;
			// we want to cancel out the current velocity, so as to knock launch them into the stratosphere
			//rb.AddForce((knockbackForce - rb.velocity) * knockbackMult, ForceMode.Impulse
			//rb.AddForce((knockbackForce.normalized * knockbackForce.magnitude), ForceMode.Impulse);

			rb.AddForce(knockbackForce - rb.velocity, ForceMode.VelocityChange);
			//this worked the best

			Debug.Log("before vel: " + rb.velocity + " " + rb.velocity.magnitude);
		}
		if (rb.velocity.sqrMagnitude > knockbackForce.sqrMagnitude)
		{
			rb.AddForce(- rb.velocity, ForceMode.Impulse);
			//this only works Using impulse, changing it to velocity change bugs it out
			Debug.Log("MAX VEL: " + rb.velocity.sqrMagnitude);
		}
	}

	virtual public void ForceKill()
	{
		OnDeath();
	}

	virtual protected void OnDeath()
	{
		SoulDropCalculation(soulValue);
		HealthDropCalculation(healthValue);

		FMODUnity.RuntimeManager.PlayOneShot(deathSound, transform.position);

		Instantiate(gibs, transform.position + Vector3.up * 1.4f, new Quaternion());
		Destroy(gameObject); // base effect is deleting object
	}
	//put it here because it wasn't overriding onDeath for non-executable enemies, likely because death was called in this function specifically
	private void SoulDropCalculation(int soulsToDrop)
	{
		while (soulsToDrop > 0)
		{
			var soulSpawnOffset = new Vector3(Random.Range(-1, 1), soulSpawnHeight, Random.Range(-1, 1));

			while (soulsToDrop >= 100)
			{
				soulSpawnOffset = new Vector3(Random.Range(-1, 1), soulSpawnHeight, Random.Range(-1, 1));
				var soulCollectable100 = Instantiate(soulCollectables[4], transform.position + soulSpawnOffset, transform.rotation) as GameObject;
				soulCollectable100.GetComponent<Rigidbody>().velocity = new Vector3(Random.Range(-1, 1), 1f, Random.Range(-1, 1)) * soulSpeed;
				Debug.Log("SoulDropped");
				soulsToDrop -= 100;
			}

			while (soulsToDrop >= 50)
			{
				soulSpawnOffset = new Vector3(Random.Range(-1, 1), soulSpawnHeight, Random.Range(-1, 1));
				var soulCollectable50 = Instantiate(soulCollectables[3], transform.position + soulSpawnOffset, transform.rotation) as GameObject;
				soulCollectable50.GetComponent<Rigidbody>().velocity = new Vector3(Random.Range(-1, 1), 1f, Random.Range(-1, 1)) * soulSpeed;
				Debug.Log("SoulDropped");
				soulsToDrop -= 50;
			}

			while (soulsToDrop >= 20)
			{
				soulSpawnOffset = new Vector3(Random.Range(-1, 1), soulSpawnHeight, Random.Range(-1, 1));
				var soulCollectable20 = Instantiate(soulCollectables[2], transform.position + soulSpawnOffset, transform.rotation) as GameObject;
				soulCollectable20.GetComponent<Rigidbody>().velocity = new Vector3(Random.Range(-1, 1), 1f, Random.Range(-1, 1)) * soulSpeed;
				Debug.Log("SoulDropped");
				soulsToDrop -= 20;
			}

			while (soulsToDrop >= 5)
			{
				soulSpawnOffset = new Vector3(Random.Range(-1, 1), soulSpawnHeight, Random.Range(-1, 1));
				var soulCollectable5 = Instantiate(soulCollectables[1], transform.position + soulSpawnOffset, transform.rotation) as GameObject;
				soulCollectable5.GetComponent<Rigidbody>().velocity = new Vector3(Random.Range(-1, 1), 1f, Random.Range(-1, 1)) * soulSpeed;
				Debug.Log("SoulDropped");
				soulsToDrop -= 5;
			}

			soulSpawnOffset = new Vector3(Random.Range(-1, 1), soulSpawnHeight, Random.Range(-1, 1));
			var soulCollectable = Instantiate(soulCollectables[0], transform.position + soulSpawnOffset, transform.rotation) as GameObject;
			soulCollectable.GetComponent<Rigidbody>().velocity = new Vector3(Random.Range(-1, 1), 1f, Random.Range(-1, 1)) * soulSpeed;
			Debug.Log("SoulDropped");
			soulsToDrop--;
		}
	}

	private void HealthDropCalculation(int healthToDrop)
	{
		while (healthToDrop > 0)
		{
			var healthSpawnOffset = new Vector3(Random.Range(-1, 1), soulSpawnHeight, Random.Range(-1, 1));
			var healthPickup = Instantiate(soulCollectables[5], transform.position + healthSpawnOffset, transform.rotation) as GameObject;
			healthPickup.GetComponent<Rigidbody>().velocity = new Vector3(Random.Range(-1, 1), 1f, Random.Range(-1, 1)) * soulSpeed;
			Debug.Log("SoulDropped");
			healthToDrop--;
		}
	}
}