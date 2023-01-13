	using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XNode.Examples.StateGraph {
	public class RaycastNode : StateNode
	{
		[Input] public float range;
		[Input] public LayerMask mask;
		[Input] public string LookingForTag;
		[Input] public Vector3 end;

		[Output] RaycastHit hitInfo;

		public override bool Evaluate()
		{
			float r = GetInputValue<float>("range", this.range);
			int msk = GetInputValue<int>("mask", this.mask);
			Vector3 p = GetInputValue<Vector3>("end", this.end);
			string tag = GetInputValue<string>("LookingForTag", this.LookingForTag);

			Vector3 start = (graph as StateGraph).currentUser.transform.position + new Vector3(0, 4.5f);
			if (Physics.Raycast(start, (p - start).normalized, out hitInfo, r, msk))
			{
				Debug.DrawLine(start, hitInfo.point, Color.cyan, 3);
				return hitInfo.transform.tag == tag;
			}
			return false;
		}
	}
}