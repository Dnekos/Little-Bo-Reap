using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepHolder : MonoBehaviour
{
	[SerializeField] protected List<Transform> containedSheep;
	[Header("Layers")]
	[SerializeField] protected int SheepLayer = 10;
	[SerializeField] protected int GroundLayer = 6;
	[SerializeField] protected float Height = -1;

	// Start is called before the first frame update
	void Start()
    {
        
    }
	public void RemoveSheep(Transform sheep)
	{
		containedSheep.Remove(sheep);
		sheep.gameObject.layer = SheepLayer;
		// make new height? maybe? or just collapse the whole thing?
		if (containedSheep.Count != 0)
			Height = containedSheep[containedSheep.Count - 1].position.y;
	}
}
