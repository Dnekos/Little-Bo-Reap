using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XNode.Examples.StateGraph
{
	public class RandomPointInCircle : StateNode
	{
		[Input] public Vector3 point;
        [Input] public float radius;

		[Output] public float result;

		public override object GetValue(NodePort port)
		{
			Vector3 p = GetInputValue<Vector3>("point", this.point);
			float r = GetInputValue<float>("radius", this.radius);

			Vector3 result = p;
			//calculate

			return result;
		}
	}
}
