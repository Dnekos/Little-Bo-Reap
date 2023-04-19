using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleObjectPooler : MonoBehaviour
{
	[SerializeField] int MaxInstancesPerPool = 40;

	Dictionary<GameObject, Queue<GameObject>> objectPool;

    // Start is called before the first frame update
    void Awake()
    {
		objectPool = new Dictionary<GameObject, Queue<GameObject>>();
	}

	public void CheckQueueExists(GameObject prefabKey)
	{
		if (!objectPool.ContainsKey(prefabKey))
		{
			Debug.Log("new pool for " + prefabKey.name + " particles");
			objectPool.Add(prefabKey, new Queue<GameObject>());
		}
	}

	public void QueuePooledObject(PoolQueuer obj)
	{
		CheckQueueExists(obj.originalPrefab);
		objectPool[obj.originalPrefab].Enqueue(obj.gameObject);
	}

	public GameObject DequeuePooledObject(GameObject prefab, Vector3 pos, Quaternion rot)
	{
		if (objectPool == null)
			objectPool = new Dictionary<GameObject, Queue<GameObject>>();
		CheckQueueExists(prefab);

		// look through pool to see if there is an eligable spawned object
		Queue<GameObject> pool = objectPool[prefab];
		Debug.Log(prefab.name +" "+pool.Count);
		if (pool.Count > 0)
		{
			GameObject cueball = pool.Dequeue();
			cueball.SetActive(true);
			cueball.GetComponent<ParticleSystem>().Play();
			cueball.transform.position = pos;
			cueball.transform.rotation = rot;
			return cueball;
		}/*for (int i = 0; i < pool.Count; i++)
		{
			if (pool[i] != null && !pool[i].activeInHierarchy)
			{
				pool[i].SetActive(true);
				pool[i].GetComponent<ParticleSystem>().Play();
				pool[i].transform.position = pos;
				pool[i].transform.rotation = rot;
				return pool[i];
			}
		}*/

		// instantiate new object
		if (MaxInstancesPerPool > pool.Count)
		{
			GameObject newPooledParticle = Instantiate(prefab, pos, rot, transform);
			//pool.Add(newPooledParticle);\
			PoolQueuer cue = newPooledParticle.GetComponent<PoolQueuer>();
			if (cue == null)
				Debug.LogError(prefab.name + " does not have a PoolQueuer. Add it to the prefab now!!");
			else
			{
				cue.originalPrefab = prefab;
				return newPooledParticle;
			}
		}
		return null;
	}

	public GameObject DequeuePooledObject(GameObject prefab)
	{
		return DequeuePooledObject(prefab, Vector3.zero, Quaternion.identity);
	}
}
