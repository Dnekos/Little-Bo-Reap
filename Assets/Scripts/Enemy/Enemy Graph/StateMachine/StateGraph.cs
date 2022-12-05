using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XNode.Examples.StateGraph {
	[CreateAssetMenu(fileName = "New State Graph", menuName = "xNode Examples/State Graph")]
	public class StateGraph : NodeGraph {

		// The current "active" node
		//public StateNode current;

		public EnemyAI currentUser = null;

		public void AnalyzeGraph(EnemyAI caller)
		{
			currentUser = caller;

			FindLeftmostNode().Evaluate();
		}

		// TODO: make this only run once on compile
		StateNode FindLeftmostNode()
		{
			Node currentleftmost = nodes[0];
			for (int i = 0; i < nodes.Count; i++)
			{
				if (nodes[i].position.x < currentleftmost.position.x)
					currentleftmost = nodes[i];
			}
			return (StateNode)currentleftmost;
		}

		public void Continue() {

		}
	}
}