using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XNode.Examples.StateGraph
{
	public class LookAtPointNode : StateNode
	{
		[Input] public Vector3 point;

		public override bool Evaluate()
		{
			Vector3 p = GetInputValue<Vector3>("point", this.point);

			Transform trn = (graph as StateGraph).currentUser.transform;
			trn.rotation = Quaternion.LookRotation(p - trn.position, Vector3.up);
			trn.eulerAngles = new Vector3(0, trn.eulerAngles.y);

			return true;
		}
	}
}