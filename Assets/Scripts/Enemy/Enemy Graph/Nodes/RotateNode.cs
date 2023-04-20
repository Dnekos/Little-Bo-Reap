using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XNode.Examples.StateGraph
{
	public class RotateNode : StateNode
	{
		[Input] public float degrees;
		[Input] public Vector3 point;


		public override bool Evaluate()
		{
			float d = GetInputValue<float>("degrees", this.degrees);
			Vector3 p = GetInputValue<Vector3>("point", this.point);
			Transform trn = (graph as StateGraph).currentUser.transform;

			Quaternion origRot = trn.rotation;
			var tempTrans = trn;
			tempTrans.rotation = Quaternion.LookRotation(p - tempTrans.position, Vector3.up);
			tempTrans.eulerAngles = new Vector3(0, tempTrans.eulerAngles.y);

			//Vector3 tempEuler = new Vector3(0, trn.eulerAngles.y + degrees);
			//Quaternion endRot =  Quaternion.LookRotation(p - trn.position, Vector3.up) * Quaternion.Euler(tempEuler);
			//(graph as StateGraph).currentUser.StartCoroutine(LerpRotation(p, trn.rotation, Quaternion.LookRotation(p - trn.position, Vector3.up)));


			//trn.rotation = endRot;
			tempTrans.eulerAngles = new Vector3(0, tempTrans.eulerAngles.y + d);

			Quaternion endRot = tempTrans.rotation;
			(graph as StateGraph).currentUser.StartCoroutine(LerpRotation(point, origRot, endRot));

			return true;
		}

		IEnumerator LerpRotation(Vector3 point, Quaternion currentRot, Quaternion endRot)
		{
			Transform trn = (graph as StateGraph).currentUser.transform;
			trn.rotation = currentRot;

			Quaternion lerpedAngle = currentRot;
			float elapsedTime = 0f;
			float waitTime = 1f;

			while (elapsedTime < waitTime)
			{
				lerpedAngle = new Quaternion(
					Mathf.LerpAngle(lerpedAngle.x, endRot.x, elapsedTime),
					Mathf.LerpAngle(lerpedAngle.y, endRot.y, elapsedTime),
					Mathf.LerpAngle(lerpedAngle.z, endRot.z, elapsedTime),
					Mathf.LerpAngle(lerpedAngle.w, endRot.w, elapsedTime));

				//Debug.Log("elapsed:" + elapsedTime);
				elapsedTime += Time.deltaTime;

				trn.rotation = lerpedAngle;
				//trn.eulerAngles = new Vector3(0, trn.eulerAngles.y);
				yield return null;
			}

			yield return null;

		}

	}
}