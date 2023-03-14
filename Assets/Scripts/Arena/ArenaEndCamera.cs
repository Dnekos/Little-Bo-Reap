using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArenaEndCamera : MonoBehaviour
{
    [SerializeField] float lifetime;
    [SerializeField] Transform lookPoint;
    Vector3 lookPosition;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void InitCamera(Transform look)
    {
        transform.LookAt(look);
    }

  
}
