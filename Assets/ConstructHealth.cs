using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructHealth : Damageable
{
	[Header("Construct Health")]
	[SerializeField] float damageMultiplier = 1;
	[SerializeField, Min(1)] int spread = 5;

	SheepConstruct construct;
	protected override void Start()
	{
		construct = GetComponent<SheepConstruct>();
	}
	public override void TakeDamage(Attack atk, Vector3 attackForward, float damageAmp = 1, float knockbackMultiplier = 0)
	{
		List<PlayerSheepAI> sheep = construct.GetContainedSheep();

		// no sheep to take damage
		if (sheep.Count <= 0)
			return;


		int workingSpread = Mathf.Min(sheep.Count, spread);
		float damageDampen = damageAmp * Mathf.Min(workingSpread, damageMultiplier) / workingSpread;
		for (int i = sheep.Count - 1; i >= sheep.Count - workingSpread; i--)
		{
			sheep[i].TakeDamage(atk, attackForward, damageDampen, PlayerSheepAI.DamageFromConstruct);
		}

		if (WorldState.instance.PersistentData.activeUpgrades.HasFlag(SaveData.Upgrades.BuilderConstructDR))
			FMODUnity.RuntimeManager.PlayOneShot(deathSound, transform.position); // death sound co-opted into compaction sound
		else
			FMODUnity.RuntimeManager.PlayOneShot(hurtSound, transform.position);

		// no sheep left after, destroy wall
		if (sheep.Count <= 0)
			Destroy(gameObject);
	}
}
