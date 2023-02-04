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
			NUM_CURRENT_ENEMIES
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

					
				}
			}
			return null;
		}

	}
}