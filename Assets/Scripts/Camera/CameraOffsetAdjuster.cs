using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// this guys is put on the player rig so that camera can be changed by animations
/// </summary>
public class CameraOffsetAdjuster : MonoBehaviour
{
	public Vector3 Offset;
	[Header("Targets")]
	public Transform LookTarget;
	[SerializeField] Transform HeadBone;
	[Header("Conditions")]
	[SerializeField] float constraint = 120;
	[SerializeField] float inSpeed = 2, outSpeed = 4;

	[Header("Events"),Tooltip("Used so that player animations can fire events outside of this gameobject"),SerializeField]
	UnityEvent[] events;

	// local vars
	float lerptime = 0;
	Quaternion targetRot; // save last rotation to lerp out

	// components
	Animator anim;

	private void Start()
	{
		anim = GetComponent<Animator>();
		targetRot = HeadBone.rotation;
	}

	public void RunEvent(int index)
	{
		events[index].Invoke();
	}

	private void LateUpdate()
	{
		if (LookTarget != null && HeadBone != null)
		{
			Vector3 toTarget = HeadBone.position - LookTarget.position;

			if (Vector3.Angle(HeadBone.forward, toTarget) <= constraint)
			{
				targetRot = Quaternion.LookRotation(toTarget, -transform.right);
				lerptime += Time.deltaTime * inSpeed;

				HeadBone.rotation = Quaternion.Slerp(HeadBone.rotation, targetRot, lerptime);
			}

		}
		else if (lerptime > 0)
		{
			// lerp back to normal
			lerptime -= Time.deltaTime * outSpeed;
			HeadBone.rotation = Quaternion.Slerp(HeadBone.rotation, targetRot, lerptime);
		}

		// keep it clamped
		Mathf.Clamp01(lerptime);
	}
	/*
	 * doesnt work with a generic rig, so sad
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

	}*/
}
