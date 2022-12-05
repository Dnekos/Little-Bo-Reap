using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XNode.Examples.StateGraph {
	public class SequenceNode : StateNode
	{
		[Output] public Empty exit;

		public override bool Evaluate()
		{
			List<NodePort> exitPort = GetOutputPort("exit").GetConnections();
			for (int i = 0; i < exitPort.Count; i++)
			{
				StateNode node = exitPort[i].node as StateNode;
				if (!node.Evaluate())
					return false;
			}
			return true;
		}
	}
}