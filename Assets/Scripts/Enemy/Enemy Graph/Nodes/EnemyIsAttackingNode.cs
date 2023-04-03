using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XNode.Examples.StateGraph
{
	public class EnemyIsAttackingNode : StateNode
	{
		[InspectorName("!="), Input] public bool not;

		public override bool Evaluate()
		{
			Animator anim = (graph as StateGraph).currentUser.GetAnimator();

			// name is the attack ones and are in the middle of the animation
			bool result = ((anim.GetCurrentAnimatorStateInfo(0).IsName("Test_Enemy_Attack_1") || anim.GetCurrentAnimatorStateInfo(0).IsName("Test_Enemy_Attack_2")) &&
				anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f);

			return result != not;
		}
	}
}