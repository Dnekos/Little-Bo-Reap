using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineFollower : MonoBehaviour
{
	[SerializeField] public EnemyFlightPath path;
	[SerializeField] public float SplinePosition = 0;
	[SerializeField] float SplineSpeed = 1;


	// Update is called once per frame
	private void Update()
	{
		UpdatePosition();
	}
	void UpdatePosition()
    {
		// 000.000
		// index.lerp t
		SplinePosition = (SplinePosition + Time.deltaTime * SplineSpeed) % path.GetPoints().Length;
		transform.position = path.GetLerpPosition(SplinePosition);
		transform.forward = path.GetLerpTangent(SplinePosition);
	}
}
