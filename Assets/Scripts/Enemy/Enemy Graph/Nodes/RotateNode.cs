using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XNode.Examples.StateGraph
{
	public class RotateNode : StateNode
	{
		[Input] public float degrees;

		public override bool Evaluate()
		{
			float p = GetInputValue<float>("degrees", this.degrees);

			Transform trn = (graph as StateGraph).currentUser.transform;
			Vector3 to = new Vector3(0, degrees, 0);
			trn.eulerAngles = Vector3.Lerp(trn.rotation.eulerAngles, to, Time.deltaTime);

			trn.eulerAngles = new Vector3(0, trn.eulerAngles.y + p);
			return true;
		}
	}
}