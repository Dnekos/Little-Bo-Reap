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
			NEARBY,
			ENEMY_FORWARD,
			ACTIVE_SHEEP

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
					case Variable.ACTIVE_SHEEP:
						//if (port.fieldName == "transformListType")
						return GameObject.FindGameObjectsWithTag("Sheep");
					//break;
					case Variable.ENEMY_FORWARD:
						//if (port.fieldName == "vectorType")
						return (graph as StateGraph).currentUser.transform.forward;
					//break;
					case Variable.ENEMY_POS:
						//if (port.fieldName == "vectorType")
						return (graph as StateGraph).currentUser.transform.position;
					//break;
					case Variable.NEARBY:
						//if (port.fieldName == "transformListType")
						return (graph as StateGraph).currentUser.NearbyGuys;
						//break;
				}
			}
			return null;
		}
		
	}
}