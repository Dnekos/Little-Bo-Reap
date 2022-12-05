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
			ACTIVE_SHEEP_AVE_POS,
			PLAYER_POS

		}
		[Input] public Variable desiredValue;
		[Output] public float result;

		public override object GetValue(NodePort port)
		{
			EnemyAI user = (graph as StateGraph).currentUser;
			if (user != null)
			{
				switch (desiredValue)
				{
					case Variable.ACTIVE_SHEEP_COUNT:
						return GameObject.FindGameObjectsWithTag("Sheep").Length;

					case Variable.ACTIVE_SHEEP_AVE_POS:
						Vector3 sheep_pos = Vector3.zero;
						GameObject[] sheep = GameObject.FindGameObjectsWithTag("Sheep");
						for (int i = 0; i < sheep.Length; i++)
						{
							sheep_pos += sheep[i].transform.position;
						}
						return sheep_pos / sheep.Length;

					case Variable.ENEMY_FORWARD:
						return (graph as StateGraph).currentUser.transform.forward;

					case Variable.ENEMY_POS:
						return (graph as StateGraph).currentUser.transform.position;

					case Variable.NEARBY_COUNT:
						return (graph as StateGraph).currentUser.NearbyGuys.Count;

					case Variable.NEARBY_AVE_POS:
						Vector3 average_pos = Vector3.zero;
						List<Transform> guys = (graph as StateGraph).currentUser.NearbyGuys;
						for (int i = 0; i < guys.Count; i++)
						{
							average_pos += guys[i].position;
						}
						return average_pos / guys.Count;

					case Variable.PLAYER_POS:
						return (graph as StateGraph).currentUser.player.position;
				}
			}
			return null;
		}
		
	}
}