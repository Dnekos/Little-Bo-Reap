using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepHolder : Interactable
{
	[SerializeField] protected List<Transform> containedSheep;
	[Header("Layers")]
	[SerializeField] protected int SheepLayer = 10;
	[SerializeField] protected int GroundLayer = 6;
	[SerializeField] protected float CurveT = -1;

	public virtual void RemoveSheep(Transform sheep)
	{
		containedSheep.Clear();
		sheep.gameObject.layer = SheepLayer;
		// make new height? maybe? or just collapse the whole thing?
		CurveT = 0;
	}
}
