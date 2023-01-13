	using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XNode.Examples.StateGraph {
	public class ExecuteAttackNode : StateNode
	{
		[Input] public EnemyAttack attack;

		public override bool Evaluate()
		{
			return (graph as StateGraph).currentUser.RunAttack(attack);
		}

	}
}