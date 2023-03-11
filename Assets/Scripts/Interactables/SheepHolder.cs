using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepHolder : Interactable
{
	[SerializeField] protected List<PlayerSheepAI> containedSheep;
	[Header("Layers")]
	[SerializeField] protected int SheepLayer = 10;
	[SerializeField] protected int GroundLayer = 6;
	[SerializeField] protected float CurveT = -1;
	[SerializeField] GameEvent endConstructEvent;

	[Header("Sheep sizing")]
	[SerializeField] protected float SheepRadiusMult = 1;

	private void Awake()
	{
		endConstructEvent.listener.AddListener(RemoveSheep);
	}
	public virtual void RemoveSheep()
	{
		containedSheep.Clear();
		// make new height? maybe? or just collapse the whole thing?
		CurveT = 0;
	}
}
