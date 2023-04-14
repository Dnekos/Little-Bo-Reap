using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleObjectPooler : MonoBehaviour
{
	[SerializeField] int MaxInstancesPerPool = 40;

	Dictionary<GameObject, List<GameObject>> objectPool;

    // Start is called before the first frame update
    void Awake()
    {
		objectPool = new Dictionary<GameObject, List<GameObject>>();
	}

	public GameObject FetchPooledObject(GameObject prefab, Vector3 pos, Quaternion rot)
	{
		if (objectPool == null)
			objectPool = new Dictionary<GameObject, List<GameObject>>();

		if (!objectPool.ContainsKey(prefab))
		{
			Debug.Log("new pool for " + prefab.name + " particles");
			objectPool.Add(prefab, new List<GameObject>());
		}

		// look through pool to see if there is an eligable spawned object
		List<GameObject> pool = objectPool[prefab];
		for (int i = 0; i < pool.Count; i++)
		{
			if (pool[i] != null && !pool[i].activeInHierarchy)
			{
				pool[i].SetActive(true);
				pool[i].GetComponent<ParticleSystem>().Play();
				pool[i].transform.position = pos;
				pool[i].transform.rotation = rot;
				return pool[i];
			}
		}

		// instantiate new object
		Debug.Log("new " + prefab.name + " pooled");
		if (MaxInstancesPerPool < pool.Count)
		{
			GameObject newPooledParticle = Instantiate(prefab, pos, rot, transform);
			pool.Add(newPooledParticle);
			return newPooledParticle;
		}
		return null;
	}

	public GameObject FetchPooledObject(GameObject prefab)
	{
		return FetchPooledObject(prefab, Vector3.zero, Quaternion.identity);
	}
}
