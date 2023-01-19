using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XNode.Examples.StateGraph {

	public class ClosestPointAtRange : Node
	{
		[Input] public Vector3 center;
		[Input] public Vector3 point;
		[Input] public float range;

		[Output] public Vector3 result;

		public override object GetValue(NodePort port)
		{
			// https://gdbooks.gitbooks.io/3dcollisions/content/Chapter1/closest_point_sphere.html

			// Get values from input connections. Fallback to field values if input is not connected
			Vector3 p = GetInputValue<Vector3>("point", this.point);
			Vector3 c = GetInputValue<Vector3>("center", this.point);
			float r = GetInputValue<float>("range", this.range);

			Vector3 sphereToPoint = p - c;
			// Normalize that vector
			sphereToPoint = Vector3.Normalize(sphereToPoint);
			// Adjust it's length to point to edge of sphere
			sphereToPoint *= r;
			// Translate into world space
			return c + sphereToPoint;
		}
		
	}
}