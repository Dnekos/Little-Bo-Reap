using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepCurve : SheepHolder
{

	[Header("Math")]
	[SerializeField] float radius;
	[SerializeField] float scale = 1;
	[SerializeField] Vector2 limits;
	public enum CurveShape
	{
		BRIDGE,
		SPIRAL
	}
	[SerializeField] CurveShape shape;

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

	private void Start()
	{
		containedSheep = new List<Transform>();
	}

	private void Update()
	{
		for (float i = limits.x; i < limits.y; i+=0.5f)
		{
			Debug.DrawLine(CalcCurvePoint(i), CalcCurvePoint(i) + Differentiate(i), Color.red, 0.1f);
			Debug.DrawLine(CalcCurvePoint(i), CalcCurvePoint(i+0.5f), Color.green, 0.1f);
		}
	}

	#region Curves
	Vector3 CalcCurvePoint(float t)
	{
		Vector3 rawPoint = Vector3.zero;
		switch (shape)
		{
			case CurveShape.BRIDGE:
				rawPoint =  CalculateBridge(t);
				break;
			case CurveShape.SPIRAL:
				rawPoint = CalculateSpiral(t);
				break;


			default:
				return Vector3.zero;
		}
		return rawPoint * scale;
	}
	Vector3 CalculateBridge(float t)
	{
		return new Vector3(t, 1 - (t - 1) * (t - 1),t);
	}
	Vector3 CalculateSpiral(float t)
	{
		return new Vector3(Mathf.Cos(4*t), t, Mathf.Sin(4 * t));
	}
	Vector3 Differentiate(float t, float h = 1e-6f)
	{
		return ((CalcCurvePoint(t + h) - CalcCurvePoint(t - h)) / (2 * h)).normalized;

	}
	#endregion

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
			SheepRadius = other.GetComponent<CapsuleCollider>().height * other.transform.lossyScale.x;

		// set height if this is the first sheep
		if (containedSheep.Count == 0)
			CurveT = limits.x;// + SheepRadius;

		// add the little guy
		AddSheep(other.transform);
	}

	void AddSheep(Transform newSheep)
	{
		float RandomCount = 0;
		// don't add sheep if the box is filled
		Plane slice = new Plane(Differentiate(CurveT), CalcCurvePoint(CurveT));
		int SheepChecked = 0;

		// math estimating the likely amount of sheep needed to check
		float V_c = Mathf.PI * radius * radius * SheepRadius * 3f;
		float V_s = 1.333f * Mathf.PI * SheepRadius * SheepRadius * SheepRadius;
		int sheeptocheck = Mathf.CeilToInt( 0.7f * V_c / V_s);

		while (CurveT < limits.y)
		{
			// try a point
			Vector3 randInCircle = Random.insideUnitSphere * (radius - SheepRadius);

			// make a guess at a good spot to place sheep, within collider bounds
			sheepPlacement = slice.ClosestPointOnPlane(randInCircle + CalcCurvePoint(CurveT));
			
			// check if the spot is filled
			bool Filled = false;
			for (int i = containedSheep.Count - 1; i >= Mathf.Max(0, containedSheep.Count - sheeptocheck); i--) 
			{
				/*
				float distToPlane = slice.GetDistanceToPoint(containedSheep[i].position);
				// we dont need to check sheep who are significantly lower than the current height
				if (distToPlane > 1.1f * SheepRadius)
					continue;*/
				SheepChecked++;

				// check the position, left/right/front/back and below the position.
				//float displacement = Vector3.Distance(sheep.position, slice.ClosestPointOnPlane(sheep.position));
				Filled = Filled || SheepIntersection(containedSheep[i], sheepPlacement);

				// stop checking if we found a filled spot
				if (Filled)
					break;
			}
			if (!Filled)
			{
				// if empty, place sheep there
				newSheep.position = sheepPlacement;
				newSheep.gameObject.layer = GroundLayer;
				containedSheep.Add(newSheep);
				Debug.Log("Sheep " + containedSheep.Count +" took " + RandomCount + " tries and checked "+ SheepChecked+" sheep");

				// set state of AI
				newSheep.GetComponent<PlayerSheepAI>()?.DoConstruct(sheepPlacement);

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
				Debug.Log("Increased height, t=" + CurveT);
				layerCount++;

				// recalculate the plane 
				slice = new Plane(Differentiate(CurveT), CalcCurvePoint(CurveT));
			}
		}
		if (RandomCount != 0)
			Debug.Log("failed to place sheep :( took "+ RandomCount);
	}

	// TODO: if collider on final sheep ends up being a capsule, will need new math for calculating displacement
	bool ContainsSheep(Transform sheep, Vector3 pos, float centerDisplacement)
	{
		float radius = (centerDisplacement == SheepRadius) ? 
			SheepRadius : // if the checking sheep is at the same height, dont do the math (else it gives NaN)
			Mathf.Sqrt((SheepRadius * SheepRadius) - (centerDisplacement * centerDisplacement)); // Pythagorean, gets radius of circle in a sphere's slice
		return Mathf.Pow(pos.x - sheep.position.x, 2) + Mathf.Pow(pos.z - sheep.position.z, 2) < radius * radius; // equation to see if point is in circle
	}

	bool SheepIntersection(Transform sheep, Vector3 pos)
	{
		return Vector3.Distance(sheep.position, pos) < (SheepRadius + SheepRadius);
	}
	#endregion
}
