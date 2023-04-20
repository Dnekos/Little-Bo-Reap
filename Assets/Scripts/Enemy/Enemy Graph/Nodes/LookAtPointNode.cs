using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XNode.Examples.StateGraph
{
	using UnityEngine;
	public class LookAtPointNode : StateNode
	{
		[Input] public Vector3 point;

		public override bool Evaluate()
		{
			Vector3 p = GetInputValue<Vector3>("point", this.point);

			Transform trn = (graph as StateGraph).currentUser.transform;
			//trn.rotation = Quaternion.LookRotation(p - trn.position, Vector3.up);
			//trn.eulerAngles = new Vector3(0, trn.eulerAngles.y);
			(graph as StateGraph).currentUser.StartCoroutine(LerpRotation(p, trn.rotation, Quaternion.LookRotation(p - trn.position, Vector3.up)));


			//(graph as StateGraph).currentUser.StartLerpRotation(p, trn.rotation, Quaternion.LookRotation(p - trn.position, Vector3.up));
			return true;
		}
		IEnumerator LerpRotation(Vector3 point, Quaternion currentRot, Quaternion endRot)
		{
			Transform trn = (graph as StateGraph).currentUser.transform;

			Quaternion lerpedAngle = trn.rotation;
			float elapsedTime = 0f;
			float waitTime = 1f;

			while (elapsedTime < waitTime)
			{
				lerpedAngle = new Quaternion(
					Mathf.LerpAngle(lerpedAngle.x, endRot.x, elapsedTime),
					Mathf.LerpAngle(lerpedAngle.y, endRot.y, elapsedTime),
					Mathf.LerpAngle(lerpedAngle.z, endRot.z, elapsedTime),
					Mathf.LerpAngle(lerpedAngle.w, endRot.w, elapsedTime));

				Debug.Log("elapsed:" + elapsedTime);
				elapsedTime += Time.deltaTime;

				trn.rotation = lerpedAngle;
				trn.eulerAngles = new Vector3(0, trn.eulerAngles.y);
				yield return null;
			}

			trn.rotation = Quaternion.LookRotation(point - trn.position, Vector3.up);
			trn.eulerAngles = new Vector3(0, trn.eulerAngles.y);

			yield return null;

		}
	}
}