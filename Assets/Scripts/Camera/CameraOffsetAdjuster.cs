using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// this guys is put on the player rig so that camera can be changed by animations
/// </summary>
public class CameraOffsetAdjuster : MonoBehaviour
{
	public Vector3 Offset;
	public Transform LookTarget;
	Animator anim;
	private void Start()
	{
		anim = GetComponent<Animator>();
	}

	private void OnAnimatorIK(int layerIndex)
	{
		Debug.Log("checking");
		if (LookTarget != null)
		{
			anim.SetLookAtWeight(1);
			anim.SetLookAtPosition(LookTarget.position);

		}
		else
			anim.SetLookAtWeight(0);

	}
}
