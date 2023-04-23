using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XNode.Examples.StateGraph
{
	public class BossIsAttacking : StateNode
	{
		[InspectorName("!="), Input] public bool not;

		public override bool Evaluate()
		{
			Animator anim = (graph as StateGraph).currentUser.GetAnimator();
			AnimatorStateInfo animstate = anim.GetCurrentAnimatorStateInfo(0);
			// name is the attack ones and are in the middle of the animation
			bool result = ((animstate.IsTag("Attack") && (animstate.normalizedTime <= 1.0f || animstate.loop)) || anim.IsInTransition(0));

			//anim.SetBool("isAttacking", true);


			return result != not;
		}
	}
}