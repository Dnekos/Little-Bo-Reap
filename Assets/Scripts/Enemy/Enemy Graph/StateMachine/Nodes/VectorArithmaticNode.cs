using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XNode.Examples.StateGraph {

	public class VectorArithmaticNode : Node
	{
		[Input] public Vector3 num1;
		public enum Operator
		{
			[InspectorName("+")] ADD,
			[InspectorName("-")] SUBTRACT,
			[InspectorName("Angle")] ANGLE,
			[InspectorName("Dist")] DISTANCE,


		}
		[Input] public Operator opp;

		[Input] public Vector3 num2;

		[Output] public float result;

		public override object GetValue(NodePort port)
		{
			// Get new a and b values from input connections. Fallback to field values if input is not connected
			Vector3 a = GetInputValue<Vector3>("num1", this.num1);
			Vector3 b = GetInputValue<Vector3>("num2", this.num2);

			// After you've gotten your input values, you can perform your calculations and return a value
			result = 0f;

			switch (opp)
			{ 
				case Operator.ADD: default: return a + b;
				case Operator.SUBTRACT: return a - b;
				case Operator.ANGLE: return Vector3.Angle(a, b);
				case Operator.DISTANCE: return Vector3.Distance(a, b);
			}
			
		}
		
	}
}