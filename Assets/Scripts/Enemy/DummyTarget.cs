using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyTarget : Damageable
{
    Vector3 targetPos;
	// Start is called before the first frame update
	override protected void Start()
    {
		base.Start();
        targetPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = targetPos;
    }
}
