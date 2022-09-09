using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepConstruct : MonoBehaviour
{
	[SerializeField] List<Transform> containedSheep;
	[SerializeField] float Height = -1;
	float SheepRadius = 1;
	[SerializeField]  int layerCount = 0;
	[SerializeField]  Vector3 sheepPlacement;

	BoxCollider box;
	private void Start()
	{
		box = GetComponent<BoxCollider>();
		containedSheep = new List<Transform>();
	}
	private void OnTriggerEnter(Collider other)
	{
		if (!other.CompareTag("Sheep"))
			return;

		other.GetComponent<Rigidbody>().isKinematic = true;
		SheepRadius = other.gameObject.GetComponent<SphereCollider>().radius * other.transform.lossyScale.x;
		if (Height == -1)
			Height = box.bounds.min.y + SheepRadius;

		float RandomCount = 0;
		while (Height + (0.5f * SheepRadius) < box.bounds.max.y)
		{
			sheepPlacement = new Vector3(transform.position.x + Random.Range(-box.bounds.extents.x, box.bounds.extents.x),
												 Height,
												 transform.position.z + Random.Range(-box.bounds.extents.z, box.bounds.extents.z));
			bool Filled = false;
			foreach (Transform sheep in containedSheep)
			{
				if (sheep.position.y < Height - 1.1f * SheepRadius)
					continue;

				Filled = Filled || ContainsSheep(sheep, sheepPlacement, Height)
					|| ContainsSheep(sheep, sheepPlacement + Vector3.right * SheepRadius, Height)
					|| ContainsSheep(sheep, sheepPlacement + Vector3.forward * SheepRadius, Height)
					|| ContainsSheep(sheep, sheepPlacement + Vector3.left * SheepRadius, Height)
					|| ContainsSheep(sheep, sheepPlacement + Vector3.down * SheepRadius, Height - SheepRadius)
					|| ContainsSheep(sheep, sheepPlacement + Vector3.back * SheepRadius, Height);
				if (Filled)
					break;
			}
			if (!Filled)
			{
				Debug.Log("took " + RandomCount + " tries");
				other.transform.position = sheepPlacement;
				containedSheep.Add(other.transform);
				other.tag = "Player";
				return;
			}
			else if (RandomCount < 40)
				RandomCount++;
			else
			{
				RandomCount = 0;
				Height += 0.5f * SheepRadius;
				layerCount++;
			}
		}
	}

	bool ContainsSheep(Transform sheep, Vector3 pos, float height)
	{
		float centerDisplacement = height - sheep.position.y;
		float radius = centerDisplacement == SheepRadius ? SheepRadius : Mathf.Sqrt((SheepRadius * SheepRadius) - (centerDisplacement * centerDisplacement));
		return Mathf.Pow(pos.x - sheep.position.x, 2) + Mathf.Pow(pos.z - sheep.position.z, 2) < radius * radius;
	}
}
