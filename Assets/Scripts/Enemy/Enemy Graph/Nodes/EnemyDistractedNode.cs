using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XNode.Examples.StateGraph
{
	public class EnemyDistractedNode : StateNode
	{
		[InspectorName("!="), Input] public bool not;

		public override bool Evaluate()
		{
			return ((graph as StateGraph).currentUser.distracted) != not;
		}
	}
}