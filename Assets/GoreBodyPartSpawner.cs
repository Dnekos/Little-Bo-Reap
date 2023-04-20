using UnityEngine;

public class GoreBodyPartSpawner : MonoBehaviour
{
    [SerializeField] Transform spawnPoint;
    [SerializeField] GameObject goreObject;
    void OnEnable()
    {
        Invoke("NoHead", 0.05f);
    }

    void NoHead()
    {
        Instantiate(goreObject, spawnPoint.position, spawnPoint.rotation);
    }
    
}
