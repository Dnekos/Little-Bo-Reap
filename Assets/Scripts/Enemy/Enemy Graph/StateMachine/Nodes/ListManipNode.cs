	using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XNode.Examples.StateGraph {

	public class ListManipNode : Node
	{
		[Input] public object[] list;

		public enum Variable
		{
			COUNT,
			AVERAGE_POS
		}
		[Input] public Variable desiredValue;


		[Output] public float result;

		public override object GetValue(NodePort port)
		{
			EnemyAI user = (graph as StateGraph).currentUser;
			if (user != null && list != null)
			{
				switch (desiredValue)
				{
					case Variable.COUNT:
						return list.Length;
					case Variable.AVERAGE_POS:
						Vector3 average_pos = Vector3.zero;
						for (int i = 0; i < list.Length; i++)
						{
							average_pos += ((Transform)list[i]).position;
						}
						return average_pos / list.Length;
				}
			}
			return null;
		}
		
	}
}