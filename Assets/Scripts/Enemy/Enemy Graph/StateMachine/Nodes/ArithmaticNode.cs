	using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XNode.Examples.StateGraph {

	public class ArithmaticNode : Node
	{
		[Input] public float num1;
		public enum Operator
		{
			[InspectorName("+")] ADD,
			[InspectorName("-")] SUBTRACT,
			[InspectorName("/")] DIVIDE,
			[InspectorName("*")] MULTIPLY,
			[InspectorName("%")] MODULO

		}
		[Input] public Operator opp;

		[Input] public float num2;

		[Output] public float result;

		public override object GetValue(NodePort port)
		{
			// Get new a and b values from input connections. Fallback to field values if input is not connected
			float a = GetInputValue<float>("a", this.num1);
			float b = GetInputValue<float>("b", this.num2);

			// After you've gotten your input values, you can perform your calculations and return a value
			result = 0f;
			if (port.fieldName == "result")
				switch (opp)
				{
					case Operator.ADD: default: result = a + b; break;
					case Operator.SUBTRACT: result = a - b; break;
					case Operator.MULTIPLY: result = a * b; break;
					case Operator.DIVIDE: result = a / b; break;
					case Operator.MODULO: result = a % b; break;

				}
			return result;

		}
		
	}
}