using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepConstruct : SheepHolder
{
	[Header("Shown for Debug Visibility")]
	float SheepRadius = 1;
	[SerializeField]  int layerCount = 0;
	[SerializeField]  Vector3 sheepPlacement;

	[Header("Settings")]
	[SerializeField, Tooltip("How many times to try placeing a sheep before raising the height")]
	int PlacementGuesses = 40;
	[SerializeField, Tooltip("How much of a sheep's radius is the height raised when a layer is filled")]
	float HeightStep = 0.5f;

	// components
	BoxCollider box;

	private void Start()
	{
		box = GetComponent<BoxCollider>();
		containedSheep = new List<Transform>();
	}

	#region Adding Sheep
	private void OnTriggerEnter(Collider other)
	{
		// only add charging sheep
		if (other.GetComponent<PlayerSheepAI>() == null || other.GetComponent<PlayerSheepAI>().GetSheepState() != SheepStates.CHARGE)
			return;


		// approximate radius
		/* 
		 * TODO: change this to be a constant once we have the sheep model, 
		 * or make this a variable on the sheep's end, 
		 * in case different ones are different sizes
		*/
		if (other.GetComponent<SphereCollider>() != null)
			SheepRadius = other.GetComponent<SphereCollider>().radius * other.transform.lossyScale.x;
		else if (other.GetComponent<CapsuleCollider>() != null)
			SheepRadius = other.GetComponent<CapsuleCollider>().radius * other.transform.lossyScale.x;

		// set height if this is the first sheep
		if (containedSheep.Count == 0)
			Height = 0 + SheepRadius;

		// add the little guy
		AddSheep(other.transform);
	}

	void AddSheep(Transform newSheep)
	{
		float RandomCount = 0;
		// don't add sheep if the box is filled
		while (Height + (0.5f * SheepRadius) < box.bounds.max.y)
		{
			// make a guess at a good spot to place sheep, within collider bounds
			sheepPlacement = new Vector3(transform.position.x + Random.Range(-box.bounds.extents.x, box.bounds.extents.x),
												 Height,
												 transform.position.z + Random.Range(-box.bounds.extents.z, box.bounds.extents.z));
			// check if the spot is filled
			bool Filled = false;
			foreach (Transform sheep in containedSheep)
			{
				// we dont need to check sheep who are significantly lower than the current height
				if (sheep.position.y < Height - 1.1f * SheepRadius)
					continue;

				// check the position, left/right/front/back and below the position.
				Filled = Filled || ContainsSheep(sheep, sheepPlacement, Height)
					|| ContainsSheep(sheep, sheepPlacement + Vector3.right * SheepRadius, Height)
					|| ContainsSheep(sheep, sheepPlacement + Vector3.forward * SheepRadius, Height)
					|| ContainsSheep(sheep, sheepPlacement + Vector3.left * SheepRadius, Height)
					|| ContainsSheep(sheep, sheepPlacement + Vector3.down * SheepRadius, Height - SheepRadius)
					|| ContainsSheep(sheep, sheepPlacement + Vector3.back * SheepRadius, Height);

				// stop checking if we found a filled spot
				if (Filled)
					break;
			}
			if (!Filled)
			{
				// if empty, place sheep there
				Debug.Log("took " + RandomCount + " tries");
				newSheep.position = sheepPlacement;
				newSheep.gameObject.layer = GroundLayer;
				containedSheep.Add(newSheep);

				// set state of AI
				newSheep.GetComponent<PlayerSheepAI>()?.DoConstruct(this);

				return;
			}
			else if (RandomCount < PlacementGuesses)
				// if filled, try again
				RandomCount++;
			else
			{
				// if done as many checks as allowed, increase height
				RandomCount = 0;
				Height += HeightStep * SheepRadius;
				layerCount++;
			}
		}
	}

	// TODO: if collider on final sheep ends up being a capsule, will need new math for calculating displacement
	bool ContainsSheep(Transform sheep, Vector3 pos, float height)
	{
		float centerDisplacement = height - sheep.position.y;
		float radius = (centerDisplacement == SheepRadius) ? 
			SheepRadius : // if the checking sheep is at the same height, dont do the math (else it gives NaN)
			Mathf.Sqrt((SheepRadius * SheepRadius) - (centerDisplacement * centerDisplacement)); // Pythagorean, gets radius of circle in a sphere's slice
		return Mathf.Pow(pos.x - sheep.position.x, 2) + Mathf.Pow(pos.z - sheep.position.z, 2) < radius * radius; // equation to see if point is in circle
	}
	#endregion
}
