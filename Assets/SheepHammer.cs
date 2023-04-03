using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

public class SheepHammer : SheepHolder
{
	[Header("Speed")]
	[SerializeField] int SheepBars = 2;
	[SerializeField] float lerpSpeed = 1;

	[Header("Sounds")]
	[SerializeField] protected FMODUnity.EventReference placeSound;
	[SerializeField] FMODUnity.EventReference explosionSound;

	Vector3 adjustedColExt; //adjusted collider extents, used for determining valid place positions

	[Header("Shown for Debug Visibility")]
	float SheepRadius = 1;
	[SerializeField] int layerCount = 0;
	int currentCollider = 0;

	[Header("Hammer Attack")]
	[SerializeField] Attack hammeratk;
	[SerializeField] Transform hammerPoint;
	[SerializeField] float damageRadius;
	[SerializeField] LayerMask enemyLayer;
	[SerializeField] GameObject ExplosionPrefab;



	[Header("Settings")]
	[SerializeField]
	float delay = 0.2f;
	[SerializeField, Tooltip("How many times to try placeing a sheep before raising the height")]
	int PlacementGuesses = 40;
	[SerializeField, Tooltip("How much of a sheep's radius is the height raised when a layer is filled")]
	float HeightStep = 0.01f;

	// component
	[Header("Components"), SerializeField]
	BoxCollider[] cols;

	protected override void Start()
	{
		base.Start();

		//cols = GetComponent<BoxCollider>();

		for (int i = 0; i < cols.Length; i++)
			cols[i].enabled = false;

		containedSheep = new List<PlayerSheepAI>();
	}

	#region Hammer Attack
	public void SpawnExplosion()
	{
		float inverseradius = 1 / damageRadius;

		//TODO: probabluy change this
		Collider[] enemies = Physics.OverlapSphere(hammerPoint.position, damageRadius, enemyLayer);
		foreach (Collider hit in enemies)
		{
			Damageable enemy = hit.GetComponent<Damageable>();

			if (enemy != null && !hit.isTrigger)
			{
				Vector3 dir = -(hammerPoint.position - hit.transform.position).normalized;
				float knockbackscale = Mathf.Max(0, 1 - (Vector3.Distance(hammerPoint.position, hit.transform.position) * inverseradius));
				enemy.TakeDamage(hammeratk, dir, 1, knockbackscale);
			}
		}
		// explosion effect
		Instantiate(ExplosionPrefab, hammerPoint.position, Quaternion.identity);
		FMODUnity.RuntimeManager.PlayOneShot(explosionSound, hammerPoint.position);
	}
	#endregion

	#region Removing Sheep
	public override void RemoveSheep()
	{
		StopAllCoroutines();
		for (int i = 0; i < cols.Length; i++)
			cols[i].enabled = false;

		base.RemoveSheep();
	}

	public void RemoveAllSheep()
	{
		for (int i = containedSheep.Count - 1; i >= 0; i--)
		{
			containedSheep[i].EndConstruct();
		}
		containedSheep.Clear();

		for (int i = 0; i < cols.Length; i++)
			cols[i].enabled = false;

		Debug.Log("calling removeSheep");
	}

	public void RemoveAll(bool StopCoroutine = true)
	{
		layerCount = 0;
		CurveT = 0;
		if (StopCoroutine)
			StopAllCoroutines();
	}
	#endregion

	#region Adding Sheep
	public override void Interact()
	{
		base.Interact();
		StopAllCoroutines();

		// starting hammer
		if (containedSheep.Count <= 0)
			StartCoroutine(AddAllSheep(WorldState.instance.player.GetComponent<PlayerSheepAbilities>().sheepFlocks, delay));
		else // ending hammer
			RemoveAllSheep();
	}
	IEnumerator AddAllSheep(Flock[] flocks, float delay)
	{
		Debug.Log("creating hammer");


		yield return new WaitForSeconds(delay);

		// reset height
		CurveT = 0;
		layerCount = 0;

		// reset collider
		currentCollider = 0;
		// find out how much that influences
		adjustedColExt = GetAdjustedColliderExtants(0);


		for (int j = 0; j < 3; j++)
		{
			List<PlayerSheepAI> curFlock = flocks[j].activeSheep;

			// if no sheep in this flock, move on
			if (curFlock.Count <= 0)
				continue;

			// find out how big a sheep is


			float origSR = SheepRadius; // keep track or non-modified for scale adjustment later
			SheepRadius *= SheepRadiusMult; // mult controls density

			Debug.Log("Col Ext: " + adjustedColExt);

			// add sheep from 
			for (int i = 0; i < curFlock.Count; i++)
			{
				FMODUnity.RuntimeManager.StudioSystem.setParameterByName("ConstructCompletion", CurveT);

				// add the little guy
				AddSheep(curFlock[i]);

				// delay if the sheep increment is right (if bars is two it does sheep 2 at a time)
				if (i % SheepBars == 0 && delay > 0)
					yield return new WaitForSeconds(delay);

				Debug.Log(CurveT + " " + currentCollider + " " + cols.Length);

				// if the collider is finished, check if there are other colliders
				if (CurveT >= 1)
				{
					CurveT = 0;
					++currentCollider;

					// if we are at the biggest collder, break out
					if (currentCollider >= cols.Length)
						break;

					// find out how much that influences
					adjustedColExt = GetAdjustedColliderExtants(currentCollider);

				}
			}

			// break out again
			if (currentCollider >= cols.Length)
				break;
		}

		// set up wall as obstacle
		for (int i = 0; i < cols.Length; i++)
			cols[i].enabled = true;

	}
	Vector3 GetAdjustedColliderExtants(int colIndex)
	{
		// find out how much that influences
		return new Vector3(Mathf.Max(0, (cols[colIndex].size.x - SheepRadius) * 0.5f),
						   Mathf.Max(0, (cols[colIndex].size.y - SheepRadius) * 0.5f),
						   Mathf.Max(0, (cols[colIndex].size.z - SheepRadius) * 0.5f));

	}

	void AddSheep(PlayerSheepAI newSheep)
	{
		float RandomCount = 0;

		int SheepChecked = 0;

		// TEMP, FIX LATER, SHOULD BE USING SPHERES IN BOX EQUATION NOW
		float radius = 0.5f * Mathf.Max(transform.localScale.x, transform.localScale.z);

		// math estimating the likely amount of sheep needed to check
		float V_c = Mathf.PI * radius * radius * SheepRadius * 3f;
		float V_s = 1.333f * Mathf.PI * SheepRadius * SheepRadius * SheepRadius;
		int sheeptocheck = Mathf.CeilToInt(0.7f * V_c / V_s);

		while (CurveT <= 1)
		{
			// make a guess at a good spot to place sheep, within -0.5,0.5
			Vector3 localPlacement = new Vector3(Random.Range(-adjustedColExt.x, adjustedColExt.x) + cols[currentCollider].center.x,
				Mathf.Lerp(-adjustedColExt.y, adjustedColExt.y, CurveT) + cols[currentCollider].center.y,
				Random.Range(-adjustedColExt.z, adjustedColExt.z) + cols[currentCollider].center.z);

			// check if the spot is filled
			bool Filled = false;
			for (int i = containedSheep.Count - 1; i >= Mathf.Max(0, containedSheep.Count - sheeptocheck); i--)
			{
				SheepChecked++;

				// check the position, using sphere intersections
				Filled = Filled || SheepIntersection(containedSheep[i].constructPos, localPlacement, newSheep.Radius, containedSheep[i].Radius);

				// stop checking if we found a filled spot
				if (Filled)
				{
					break;
				}
					
			}

			if (!Filled) // if empty, place sheep there
			{
				containedSheep.Add(newSheep);
				Debug.Log("Sheep " + containedSheep.Count + " took " + RandomCount + " tries and checked " + SheepChecked + " sheep");

				// set state of AI
				newSheep?.DoConstruct(localPlacement);

				StartCoroutine(LerpSheep(newSheep.transform, localPlacement));

				return;
			}
			else if (RandomCount < PlacementGuesses)
				// if filled, try again
				RandomCount++;
			else
			{
				// if done as many checks as allowed, increase height
				RandomCount = 0;
				CurveT += HeightStep;// * SheepRadius;
				layerCount++;

				FMODUnity.RuntimeManager.StudioSystem.setParameterByName("ConstructCompletion", CurveT);
			}
		}
		if (RandomCount != 0)
			Debug.Log("failed to place sheep :( took " + RandomCount);
	}

	IEnumerator LerpSheep(Transform newSheep, Vector3 SheepPlacement)
	{
		Vector3 oldpos = newSheep.position, localplacement = newSheep.position;

		// keep going, incrementing t, while the sheep exists
		for (float t = 0; newSheep != null; t += Time.deltaTime * lerpSpeed)
		{
			if (newSheep == null)
				yield break;

			localplacement = transform.localToWorldMatrix.MultiplyPoint3x4(SheepPlacement);
			newSheep.position = Vector3.Lerp(oldpos, localplacement, t);
			yield return new WaitForEndOfFrame();

		} 

		if (newSheep != null)
		{
			newSheep.position = localplacement;
			newSheep.eulerAngles = Random.insideUnitSphere * 360;
			FMODUnity.RuntimeManager.PlayOneShotAttached(placeSound, newSheep.gameObject);

		}

	}

	bool SheepIntersection(Vector3 checkingPos, Vector3 newSheepPos, float selfRad, float targetRad)
	{
		return Vector3.Distance(checkingPos, newSheepPos) < (selfRad + targetRad) * SheepRadiusMult;
	}
	#endregion
}
