using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : Damageable
{

	[Header("Hitstun"), Tooltip("Prevents movement Inputs and speed checking while active")]
	public bool HitStunned = false;
	[SerializeField]
	float hitstunTimer = 0.2f;

	PlayerMovement playermove;

	// Start is called before the first frame update
	protected override void Start()
	{
		base.Start();
		playermove = GetComponent<PlayerMovement>();

	}

	public override void TakeDamage(Attack atk, Vector3 attackForward)
	{
		// stop moving, to hopefully prevent too wacky knockback
		rb.AddForce(-rb.velocity, ForceMode.VelocityChange);

		base.TakeDamage(atk, attackForward);
		if (atk.DealsHitstun)
		{
			StopCoroutine("HitstunTracker");
			StartCoroutine("HitstunTracker");
		}
	}
	IEnumerator HitstunTracker()
	{
		HitStunned = true;
		yield return new WaitForSeconds(hitstunTimer);
		HitStunned = false;
	}
}