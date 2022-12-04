using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace XNode.Examples.StateGraph {
	public class StateNode : Node {

		[Input] public Empty enter;

		public override object GetValue(NodePort port)
		{
			return position.y;
		}

		public virtual bool Evaluate()
		{
			return true;
		}

		public void SortExits()
		{
			// gather current ports
			NodePort exitPort = GetOutputPort("exit");
			List<NodePort> ports = exitPort.GetConnections();

			// sort based on height
			ports.Sort(delegate (NodePort x, NodePort y) {	return x.node.position.y.CompareTo(y.node.position.y); });
			
			// re-add connections
			exitPort.ClearConnections();
			for (int i = 0; i < ports.Count; i++)
				exitPort.Connect(ports[i]);
		}

		[Serializable]
		public class Empty { }
	}
}