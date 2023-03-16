using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaEndCamera : MonoBehaviour
{
    [SerializeField] float lifetime;
    [SerializeField] Transform lookPoint;
    [SerializeField] float spawnRadiusMin = 5f;
    [SerializeField] float spawnRadiusMax = 20f;
    [SerializeField] float ySpawnOffsetMin = 1;
    [SerializeField] float ySpawnOffsetMax = 6;
    Vector3 lookPosition;

    // Start is called before the first frame update
    void Start()
    {
       Destroy(gameObject, lifetime);
       WorldState.instance.HUD.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        WorldState.instance.HUD.gameObject.SetActive(true);
    }

    public void InitCamera(Transform look, Vector3 centerPos)
    {
        float spawnRadius = Random.Range(spawnRadiusMin, spawnRadiusMax);
        float ySpawnOffset = Random.Range(ySpawnOffsetMin, ySpawnOffsetMax);

        float x = Mathf.Cos(360) * spawnRadius + centerPos.x;
        float y = centerPos.y + ySpawnOffset;
        float z = Mathf.Sin(360) * spawnRadius + centerPos.z;

        Vector3 newPos = new Vector3(x, y, z);

        transform.position = newPos;
        transform.LookAt(look);

    }

  
}
