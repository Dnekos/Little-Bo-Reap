using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XNode.Examples.StateGraph
{
	public class RandomPointInCircle : StateNode//probably should rename this
	{

		[Output] public float result;

		public override object GetValue(NodePort port)
		{
			Vector3 p = (graph as StateGraph).currentUser.GetComponent<BabaYagasHouseAI>().spawnPoint;
			float r = (graph as StateGraph).currentUser.GetComponent<BabaYagasHouseAI>().movementRadius;
			//REVIEW: using random point in unit sphere will make it incredibly unlikely to 
			//hit the outside ring as most points of a sphere from the top will be in the center
			Vector3 randomPosition = UnityEngine.Random.insideUnitCircle * 100f;//hopefully using cicle instead of sphere will help out with movement a little
			Vector3 destinationPosition = new Vector3(randomPosition.x + p.x, p.y, randomPosition.z + p.z);

			Vector3 result = destinationPosition;
			//calculate
			//Debug.Log(result);

			return result;
		}
	}
}
