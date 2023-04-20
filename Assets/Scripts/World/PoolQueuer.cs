using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolQueuer : MonoBehaviour
{
	//[HideInInspector]
	public GameObject originalPrefab;

	private void OnDisable()
	{
		if (gameObject != null)
		{
			if (originalPrefab == null)
				Debug.LogError(gameObject.name + " is missing prefab reference!!");
			else if (WorldState.instance.isActiveAndEnabled)
				WorldState.instance.pools.QueuePooledObject(this);
		}
	}
}
