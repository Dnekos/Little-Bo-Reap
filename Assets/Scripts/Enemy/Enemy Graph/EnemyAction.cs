using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class EnemyAction : Node {

	// Use this for initialization
	protected override void Init() {
		base.Init();
		
	}

	[Input] public float a;
	[Input] public EnemyAI enemy;
	[Output] public float b;

	public override object GetValue(NodePort port)
	{
		if (port.fieldName == "b") return GetInputValue<float>("a", a);
		else return null;
	}
}