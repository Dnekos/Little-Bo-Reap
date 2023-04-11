using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace XNode.Examples.StateGraph {
	public class AgentPresetNode : StateNode
	{
		[Input] public float speed;
		[Input] public float acceleration;
		[Input] public float stoppingDist;

		public override bool Evaluate()
		{
			NavMeshAgent agent = (graph as StateGraph).currentUser.GetAgent();
			agent.speed = speed;
			agent.acceleration = acceleration;
			agent.stoppingDistance = stoppingDist;

			// make sure agent is NOT moving if we want them to stop
			agent.isStopped = speed == 0;
			if (agent.isStopped)
				agent.velocity = Vector3.zero;

			return true;
		}
	}
}