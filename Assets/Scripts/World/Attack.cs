using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Attack", menuName = "ScriptableObjects/Attack", order = 1)]
public class Attack : ScriptableObject
{
	[Header("BaseVariables")]
	public float damage;
	public float airborneLift; // possibly obsolete
	public string animation;
	public bool DealsHitstun = true;
	public bool ShowNumber = true;

	[Header("Knockback")]
	public float forwardKnockback;
	public float upwardKnockback;
}
