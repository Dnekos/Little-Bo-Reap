using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Execution", menuName = "ScriptableObjects/Execution", order = 3)]
public class Execution : ScriptableObject
{
	[Header("BaseVariables")]
	public string playerAnimation;
	public string enemyAnimation;
}
