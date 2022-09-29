using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class Shockwave : MonoBehaviour
{
	[SerializeField] Attack ShockWaveAttack;

	[Header("TransformProperties"), SerializeField]
	float maxTimeAlive = 2;
	[SerializeField]
	Vector3 maxScale;
	[SerializeField, Tooltip("HIGHLY dependant on torus shape and scale, dont touch this or torus shape :)")]
	float InnerDiameter = 3.5f;

	List<Damageable> hitTargets;

	Vector3 origPos;

	float currTimeAlive = 0;

	private void Start()
	{
		hitTargets = new List<Damageable>();

		origPos = transform.position; 
	}
	// Update is called once per frame
	void Update()
    {
		// scale over time
		currTimeAlive += Time.deltaTime;
		transform.localScale = Vector3.Lerp(Vector3.one, maxScale, currTimeAlive / maxTimeAlive);

		// probuilder is fuck-y and translates when scaled, so this counters that

		transform.position = origPos + new Vector3(0, (transform.localScale.y - 1) * 0.08674145f, (transform.localScale.z - 1) * -0.9176273f);


		if (currTimeAlive >= maxTimeAlive)
			Destroy(gameObject);
	}
	private void OnTriggerEnter(Collider other)
	{
		Vector3 flattenedOtherPos = new Vector3(other.transform.position.x, transform.position.y, other.transform.position.z);
		Damageable targetHealth = other.GetComponent<Damageable>();

		//Debug.Log(Vector3.Distance(flattenedOtherPos, transform.position) + " < " + (InnerDiameter * transform.localScale.x * 0.5f));
		
		// distance check to see if the player is in the hitbox or inside the inner radius (as triggers have to be convex)
		if (Vector3.Distance(flattenedOtherPos, origPos) < InnerDiameter * transform.localScale.x * 0.5f)
		{
			Debug.Log(other.gameObject.name + " safe inside shockwave");
		}
		else if (targetHealth != null && !hitTargets.Contains(targetHealth))
		{
			hitTargets.Add(targetHealth);
			Debug.Log(other.gameObject.name + " hit by shockwave");
			targetHealth.TakeDamage(ShockWaveAttack, (flattenedOtherPos - transform.position).normalized);

		}
	}
}

