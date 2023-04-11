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
			Debug.Log(anim.GetCurrentAnimatorStateInfo(0).GetType().Name.ToString());
			// name is the attack ones and are in the middle of the animation
			bool result = ((anim.GetCurrentAnimatorStateInfo(0).IsName("Baba_Yagas_House_Move") || 
							anim.GetCurrentAnimatorStateInfo(0).IsName("Boss_Ranged_Attack")    ||
							anim.GetCurrentAnimatorStateInfo(0).IsName("Boss_Flamethower")     ||
							anim.GetCurrentAnimatorStateInfo(0).IsName("Boss_Stomp")			||
							anim.GetCurrentAnimatorStateInfo(0).IsName("BBYGH_Perch 1") ||
							anim.GetCurrentAnimatorStateInfo(0).IsName("Boss_Spawn")			||
							anim.GetCurrentAnimatorStateInfo(0).IsName("BBYGH_Fire_Start 1")	||
							anim.GetCurrentAnimatorStateInfo(0).IsName("BBYGH_Fire_Cycle 1")||
							anim.GetCurrentAnimatorStateInfo(0).IsName("BBYGH_Spawn_Start 1") || 
							anim.GetCurrentAnimatorStateInfo(0).IsName("BBYGH_Spawn_Cycle 1")&&
							anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f));

			//anim.SetBool("isAttacking", true);


			return result != not;
		}
	}
}