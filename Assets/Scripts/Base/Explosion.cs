using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
	public Attack activeAttack;
	List<Damageable> enemiesHitThisAttack;

	private void Start()
	{
		enemiesHitThisAttack = new List<Damageable>();
	}

	private void OnTriggerEnter(Collider other)
	{
		// we use in parent cause the collider may be on an child due to amimations
		Damageable enemy = other.GetComponentInParent<Damageable>();
		if (enemy != null && !enemiesHitThisAttack.Contains(enemy)) // make sure we dont double tap
		{
			Vector3 attackVector = enemy.transform.position - transform.position;
			enemy.TakeDamage(activeAttack, attackVector.normalized);
			enemiesHitThisAttack.Add(enemy);
		}
	}
}
