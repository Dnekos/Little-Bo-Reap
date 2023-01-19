	using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XNode.Examples.StateGraph {
	public class EnemyMoveToNode : StateNode
	{
		[Input] public Vector3 destination;

		public override bool Evaluate()
		{
			Debug.Log(name);
			Vector3 a = GetInputValue<Vector3>("destination", this.destination);
			return (graph as StateGraph).currentUser.SetDestination(a);
		}
	}
}