	using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XNode.Examples.StateGraph {

	public class EqualityNode : StateNode
	{
		[Input] public float num1;
		public enum Operator
		{
			[InspectorName(">")] GREATER_THAN,
			[InspectorName("<")] LESS_THAN,
			[InspectorName("==")] EQUAL,
			[InspectorName("!=")] NOT_EQUAL
		}
		[Input] public Operator opp;

		[Input] public float num2;

		public override bool Evaluate()
		{
			float a = GetInputValue<float>("num1", this.num1);
			float b = GetInputValue<float>("num2", this.num2);

			switch (opp)
			{
				case Operator.EQUAL:
					return num1 == num2;
				case Operator.GREATER_THAN:
					return num1 > num2;
				case Operator.LESS_THAN:
					return num1 < num2;
				case Operator.NOT_EQUAL:
					return num1 != num2;
			}
			return false;
		}
	}
}