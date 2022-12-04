using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XNode.Examples.StateGraph {
	[CreateAssetMenu(fileName = "New State Graph", menuName = "xNode Examples/State Graph")]
	public class StateGraph : NodeGraph {

		// The current "active" node
		public StateNode current;

		public EnemyAI currentUser = null;

		public void AnalyzeGraph(EnemyAI caller)
		{
			currentUser = caller;
			current.Evaluate();
		}

		public StateGraph()
		{
			//current = (StateNode)nodes[0];
		}

		public void Continue() {

		}
	}
}