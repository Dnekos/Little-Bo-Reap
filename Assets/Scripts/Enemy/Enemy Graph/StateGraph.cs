using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XNode.Examples.StateGraph {
	[CreateAssetMenu(fileName = "New State Graph", menuName = "xNode Examples/State Graph")]
	public class StateGraph : NodeGraph {

		// The current "active" node
		//public StateNode current;

		public EnemyAI currentUser = null;
		public StateNode LeftMost = null;


		public void AnalyzeGraph(EnemyAI caller)
		{
			currentUser = caller;

			if (LeftMost == null)
				FindLeftmostNode();
			if (LeftMost != null)
				LeftMost.Evaluate();
			else
				Debug.LogError("Missing LeftMost!");
		}

		// TODO: make this only run once on compile
		public StateNode FindLeftmostNode()
		{
			if (nodes.Count <= 0)
				Debug.LogError("Stategraph " + this.name + " has no nodes!");
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