﻿	using System;
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
			return true;
		}
	}
}