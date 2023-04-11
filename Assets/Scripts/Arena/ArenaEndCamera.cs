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
    [SerializeField] float collideCheckRadius = 0.25f;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Vector3 lookYOffset;
    int currentChecks;
    [SerializeField] int maxChecks = 10;
    Vector3 lookPosition;

	// Start is called before the first frame update
	void Start()
	{
		Destroy(gameObject, lifetime);
		WorldState.instance.HUD.gameObject.SetActive(false);
		WorldState.instance.DisableControls();
	}

	private void OnDestroy()
    {
        WorldState.instance.HUD.gameObject.SetActive(true);
		WorldState.instance.EnableControls();

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
        look.transform.position += lookYOffset;
        transform.LookAt(look);

        if(Physics.CheckSphere(transform.position, collideCheckRadius, groundLayer) && currentChecks < maxChecks)
        {
            Debug.LogWarning("Camera is in something! trying again");
            currentChecks++;
            InitCamera(look, centerPos);
        }

    }

  
}
