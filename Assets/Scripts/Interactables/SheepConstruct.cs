using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SheepConstruct : SheepHolder
{
	[Header("Speed")]
	[SerializeField] int SheepBars = 2;
	[SerializeField] float lerpSpeed = 1;

	[Header("Sounds")]
	[SerializeField] protected FMODUnity.EventReference placeSound;

	[Header("Math")]
	[SerializeField] float h;
	[SerializeField] float w, l;

	[Header("Shown for Debug Visibility")]
	float SheepRadius = 1;
	[SerializeField] int layerCount = 0;

	[Header("Settings")]
	[SerializeField]
	float delay = 0.2f;
	[SerializeField, Tooltip("How many times to try placeing a sheep before raising the height")]
	int PlacementGuesses = 40;
	[SerializeField, Tooltip("How much of a sheep's radius is the height raised when a layer is filled")]
	float HeightStep = 0.01f;

	[Header("Collider")]

	[SerializeField] BoxCollider col;

	// components

	protected override void Start()
	{
		base.Start();

		col.enabled = false;

		containedSheep = new List<Transform>();
	}

	public override void RemoveSheep()
	{
		StopAllCoroutines();

		base.RemoveSheep();
	}

	public void RemoveAllSheep()
	{
		for (int i = containedSheep.Count - 1; i >= 0; i--)
		{
			containedSheep[i].GetComponent<PlayerSheepAI>().EndConstruct(false);
		}
		containedSheep.Clear();

		Debug.Log("calling removeSheep");

		//Destroy(gameObject);
	}

	public void RemoveAll(bool StopCoroutine = true)
	{
		layerCount = 0;
		CurveT = 0;
		if (StopCoroutine)
			StopAllCoroutines();
	}

	#region Adding Sheep
	public override void Interact()
	{
		base.Interact();
		StopAllCoroutines();
		StartCoroutine(AddAllSheep(WorldState.instance.player.GetComponent<PlayerSheepAbilities>().sheepFlocks[(int)SheepTypes.BUILD].activeSheep, delay));
	}
	IEnumerator AddAllSheep(List<PlayerSheepAI> flock, float delay)
	{
		Debug.Log("adding sheep");

		RemoveAll(false);
		RemoveAllSheep();

		yield return new WaitForSeconds(delay);

		for (int i = 0; i < flock.Count; i++)
		{
			if (flock[i].GetComponent<SphereCollider>() != null)
				SheepRadius = flock[i].GetComponent<SphereCollider>().radius * flock[i].transform.lossyScale.x;
			else if (flock[i].GetComponent<CapsuleCollider>() != null)
				SheepRadius = flock[i].GetComponent<CapsuleCollider>().radius * flock[i].transform.lossyScale.x;

			// set height if this is the first sheep
			if (containedSheep.Count == 0)
			{
				CurveT = Mathf.InverseLerp(0, col.bounds.extents.y, SheepRadius);// + SheepRadius;
				FMODUnity.RuntimeManager.StudioSystem.setParameterByName("ConstructCompletion", CurveT);
				Debug.Log("meh");
			}

			// add the little guy
			AddSheep(flock[i].transform);

			// delay if the sheep increment is right (if bars is two it does sheep 2 at a time)
			if (i % SheepBars == 0 && delay > 0)
				yield return new WaitForSeconds(delay);

			// if the curve is finished, stop counting
			if (CurveT >= 1)
			{
				break;
			}

		}
	}

	void AddSheep(Transform newSheep)
	{
		float RandomCount = 0;

		int SheepChecked = 0;

		// TEMP, FIX LATER, SHOULD BE USING SPHERES IN BOX EQUATION NOW
		float radius = 0.5f * Mathf.Min(w, l);

		// math estimating the likely amount of sheep needed to check
		float V_c = Mathf.PI * radius * radius * SheepRadius * 3f;
		float V_s = 1.333f * Mathf.PI * SheepRadius * SheepRadius * SheepRadius;
		int sheeptocheck = Mathf.CeilToInt(0.7f * V_c / V_s);

		while (CurveT <= 1)
		{
			// make a guess at a good spot to place sheep, within collider bounds
			float halfWidth = col.bounds.extents.x, halfLength = col.bounds.extents.z;
			if (halfWidth > SheepRadius)
				halfWidth -= SheepRadius;
			if (halfLength > SheepRadius)
				halfLength -= SheepRadius;

			Vector3 sheepPlacement = transform.position + new Vector3(Random.Range(-halfWidth, halfWidth), 
				Mathf.Lerp(-col.bounds.extents.y, col.bounds.extents.y, CurveT), 
				Random.Range(-halfLength, halfLength));

			// check if the spot is filled
			bool Filled = false;
			for (int i = containedSheep.Count - 1; i >= Mathf.Max(0, containedSheep.Count - sheeptocheck); i--)
			{

				SheepChecked++;

				// check the position, using sphere intersections
				Filled = Filled || SheepIntersection(containedSheep[i].GetComponent<PlayerSheepAI>().constructPos, sheepPlacement);

				// stop checking if we found a filled spot
				if (Filled)
					break;
			}
			if (!Filled) // if empty, place sheep there
			{
				containedSheep.Add(newSheep);
				Debug.Log("Sheep " + containedSheep.Count + " took " + RandomCount + " tries and checked " + SheepChecked + " sheep");

				// set state of AI
				newSheep.GetComponent<PlayerSheepAI>()?.DoConstruct(sheepPlacement);

				StartCoroutine(LerpSheep(newSheep, sheepPlacement));

				return;
			}
			else if (RandomCount < PlacementGuesses)
				// if filled, try again
				RandomCount++;
			else
			{
				// if done as many checks as allowed, increase height
				RandomCount = 0;
				CurveT += HeightStep * SheepRadius;
				layerCount++;

				FMODUnity.RuntimeManager.StudioSystem.setParameterByName("ConstructCompletion", CurveT);
			}
		}
		if (RandomCount != 0)
			Debug.Log("failed to place sheep :( took " + RandomCount);
	}

	IEnumerator LerpSheep(Transform newSheep, Vector3 SheepPlacement)
	{
		Vector3 oldpos = newSheep.position;

		float t = 0;
		do
		{
			if (newSheep == null)
				yield break;

			t += Time.deltaTime * lerpSpeed;
			newSheep.position = Vector3.Lerp(oldpos, SheepPlacement, t);
			yield return new WaitForEndOfFrame();

		} while (t < 1 && newSheep != null);

		if (newSheep != null)
		{
			newSheep.position = SheepPlacement;
			newSheep.eulerAngles = Random.insideUnitSphere * 360;
			FMODUnity.RuntimeManager.PlayOneShotAttached(placeSound, newSheep.gameObject);

		}

	}

	// TODO: if collider on final sheep ends up being a capsule, will need new math for calculating displacement
	bool ContainsSheep(Transform sheep, Vector3 pos, float centerDisplacement)
	{
		float radius = (centerDisplacement == SheepRadius) ?
			SheepRadius : // if the checking sheep is at the same height, dont do the math (else it gives NaN)
			Mathf.Sqrt((SheepRadius * SheepRadius) - (centerDisplacement * centerDisplacement)); // Pythagorean, gets radius of circle in a sphere's slice
		return Mathf.Pow(pos.x - sheep.position.x, 2) + Mathf.Pow(pos.z - sheep.position.z, 2) < radius * radius; // equation to see if point is in circle
	}

	bool SheepIntersection(Vector3 checkingPos, Vector3 newSheepPos)
	{
		return Vector3.Distance(checkingPos, newSheepPos) < (SheepRadius + SheepRadius);
	}
	#endregion
}
