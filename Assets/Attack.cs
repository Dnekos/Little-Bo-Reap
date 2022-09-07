using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Attack", menuName = "ScriptableObjects/Attack", order = 1)]
public class Attack : ScriptableObject
{
	[Header("Light Attack Variables")]
	public float damage;
	public float airborneLift;
	public string animation;

	public float forwardKnockback;
	public float upwardKnockback;
}
