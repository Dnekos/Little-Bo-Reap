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
			[InspectorName("Angle")] ANGLE

		}
		[Input] public Operator opp;

		[Input] public Vector3 num2;

		[Output] public Vector3 vResult;
		[Output] public float fResult;

		public override object GetValue(NodePort port)
		{
			// Get new a and b values from input connections. Fallback to field values if input is not connected
			Vector3 a = GetInputValue<Vector3>("num1", this.num1);
			Vector3 b = GetInputValue<Vector3>("num2", this.num2);

			// After you've gotten your input values, you can perform your calculations and return a value
			fResult = 0f;
			vResult = Vector3.zero;

			if (port.fieldName == "vResult")
			{
				switch (opp)
				{
					case Operator.ADD: default: vResult = a + b; break;
					case Operator.SUBTRACT: vResult = a - b; break;
				}
				return vResult;
			}
			else if (port.fieldName == "fResult" && opp == Operator.ANGLE)
			{
				fResult = Vector3.Angle(a, b);
				return fResult;
			}
			return null;
		}
		
	}
}