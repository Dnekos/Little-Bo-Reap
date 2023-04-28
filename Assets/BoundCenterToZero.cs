using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundCenterToZero : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = transform.parent.position + (transform.position - GetComponent<Collider>().bounds.center);
    }
}
