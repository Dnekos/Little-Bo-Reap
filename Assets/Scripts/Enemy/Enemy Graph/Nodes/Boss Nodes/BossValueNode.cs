using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XNode.Examples.StateGraph
{

	public class BossValueNode : Node
	{
		public enum BossVariables
		{
			NUM_CURRENT_ENEMIES,
			CURRENT_HEATH,
			MAX_HEALTH,
			ENEMIES_SPAWNED,
			IS_MOVING
		}
		[Input] public BossVariables desiredValue;
		[Output] public float result;

		public override object GetValue(NodePort port)
		{
			EnemyAI user = (graph as StateGraph).currentUser;
			if (user != null)
			{
				switch (desiredValue)
				{
					case BossVariables.NUM_CURRENT_ENEMIES:
						return GameObject.FindGameObjectsWithTag("Enemy").Length;

					case BossVariables.CURRENT_HEATH:
						return user.GetComponent<BabaYagasHouseAI>().GetHeath();//is this the best way to do this?

					case BossVariables.MAX_HEALTH:
						return user.GetComponent<BabaYagasHouseAI>().GetMaxHeath();

					case BossVariables.ENEMIES_SPAWNED:
						return user.GetComponent<BabaYagasHouseAI>().getEnemiesSpawned();

					case BossVariables.IS_MOVING:
                    {
						AnimatorClipInfo[] currentClipInfo = user.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0);
						string clipName = currentClipInfo[0].clip.name;
							
						if(clipName != "Boss_Idle")
                        {
							return true;
                        }
                        else
                        {
							return false;
                        }
					}
				}
			}
			return null;
		}

	}
}