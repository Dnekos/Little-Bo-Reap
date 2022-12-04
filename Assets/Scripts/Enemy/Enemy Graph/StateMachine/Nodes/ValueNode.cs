	using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XNode.Examples.StateGraph {

	public class ValueNode : Node
	{
		public enum Variable
		{
			ENEMY_POS,
			NEARBY_COUNT,
			NEARBY_AVE_POS,
			ENEMY_FORWARD,
			ACTIVE_SHEEP_COUNT,
			ACTIVE_SHEEP_AVE_POS

		}
		[Input] public Variable desiredValue;


		[Output] public int result;
		/*[Output] public float floatType;
		[Output] public int intType;
		[Output] public Vector3 vectorType;
		[Output] public List<Transform> transformListType;
		//[Output] public Type resultType;*/
		public override object GetValue(NodePort port)
		{
			EnemyAI user = (graph as StateGraph).currentUser;
			if (user != null)
			{
				switch (desiredValue)
				{
					case Variable.ACTIVE_SHEEP_COUNT:
						//if (port.fieldName == "transformListType")
						return GameObject.FindGameObjectsWithTag("Sheep").Length;
					case Variable.ACTIVE_SHEEP_AVE_POS:
						//if (port.fieldName == "transformListType")
						Vector3 sheep_pos = Vector3.zero;
						GameObject[] sheep = GameObject.FindGameObjectsWithTag("Sheep");
						for (int i = 0; i < sheep.Length; i++)
						{
							sheep_pos += sheep[i].transform.position;
						}
						return sheep_pos / sheep.Length;

					//break;
					case Variable.ENEMY_FORWARD:
						//if (port.fieldName == "vectorType")
						return (graph as StateGraph).currentUser.transform.forward;
					//break;
					case Variable.ENEMY_POS:
						//if (port.fieldName == "vectorType")
						return (graph as StateGraph).currentUser.transform.position;
					//break;
					case Variable.NEARBY_COUNT:
						//if (port.fieldName == "transformListType")
						return (graph as StateGraph).currentUser.NearbyGuys.Count;
					case Variable.NEARBY_AVE_POS:
						//if (port.fieldName == "transformListType")
						Vector3 average_pos = Vector3.zero;
						List<Transform> guys = (graph as StateGraph).currentUser.NearbyGuys;
						for (int i = 0; i < guys.Count; i++)
						{
							average_pos += guys[i].position;
						}
						return average_pos / guys.Count;
					//break;
				}
			}
			return null;
		}
		
	}
}