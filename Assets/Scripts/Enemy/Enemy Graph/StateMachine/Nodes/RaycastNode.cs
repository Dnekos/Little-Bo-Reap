	using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XNode.Examples.StateGraph {
	public class RaycastNode : StateNode
	{
		[Input] public float range;
		[Input] public LayerMask mask;
		[Input] public Vector3 end;

		[Output] RaycastHit hitInfo;

		public override bool Evaluate()
		{
			Vector3 start = (graph as StateGraph).currentUser.transform.position;
			return Physics.Raycast(start, end - start, range, mask);
		}

		public override object GetValue(NodePort port)
		{
			Vector3 start = (graph as StateGraph).currentUser.transform.position;
			Physics.Raycast(start, end - start, out hitInfo, range, mask);

			return hitInfo;
		}
	}
}