using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XNode.Examples.StateGraph {

	public class EqualityNode : StateNode
	{
		[Input] public float lhs;
		public enum Operator
		{
			[InspectorName(">")] GREATER_THAN,
			[InspectorName("<")] LESS_THAN,
			[InspectorName("==")] EQUAL,
			[InspectorName("!=")] NOT_EQUAL
		}
		[Input] public Operator opp;

		[Input] public float rhs;

		public override bool Evaluate()
		{
			float a = lhs, b = rhs;
			if (GetInputPort("lhs").IsConnected)
				 a = Convert.ToSingle(GetInputPort("lhs").Connection.GetOutputValue());
			if (GetInputPort("rhs").IsConnected)
				b = Convert.ToSingle(GetInputPort("rhs").Connection.GetOutputValue());

			// these two dont work for some reason, idk
			//float a = GetInputValue<float>("lhs", this.lhs);
			//float b = GetInputValue<float>("rhs", this.rhs);
			bool result = false;

			switch (opp)
			{
				case Operator.EQUAL:
					result = a == b;
					break;
				case Operator.GREATER_THAN:
					result = a > b;
					break;
				case Operator.LESS_THAN:
					result = a < b;
					break;
				case Operator.NOT_EQUAL:
					result = a != b;
					break;
			}
			return result;
		}
	}
}