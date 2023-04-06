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
		[Input] public Vector3 enemyPosition;

		[Output] public float result;

		public override object GetValue(NodePort port)
		{
			Vector3 p = GetInputValue<Vector3>("point", this.point);
			float r = GetInputValue<float>("radius", this.radius);
			//REVIEW: using random point in unit sphere will make it incredibly unlikely to 
			//hit the outside ring as most points of a sphere from the top will be in the center
			Vector3 randomPosition = UnityEngine.Random.insideUnitCircle * r;//hopefully using cicle instead of sphere will help out with movement a little
			Vector3 destinationPosition = new Vector3(randomPosition.x + p.x, p.y, randomPosition.z + p.z);

			Vector3 result = destinationPosition;
			//calculate
			//Debug.Log(result);


			return result;
		}
	}
}
