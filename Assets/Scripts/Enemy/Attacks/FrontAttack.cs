using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Attack", menuName = "ScriptableObjects/Enemy Frontal Attack", order = 2)]
public class FrontAttack : EnemyAttack
{
	[Header("Attack Condition"), SerializeField]
	float AngleWindow = 90;

	public override bool CheckCondition(Transform user, Transform player, List<Transform> NearbyGuys)
	{
		if (NearbyGuys.Count == 0)
			return false;

		Vector3 average_pos = Vector3.zero;
		for (int i = 0; i < NearbyGuys.Count; i++)
		{
			average_pos += NearbyGuys[i].position;
		}

		// check to see if most of the sheep are in front of him TODO, make this more modular
		Vector3 heading = (average_pos / NearbyGuys.Count) - user.position;
		float angle = Vector3.Angle(user.forward, heading);


		return angle < AngleWindow;
	}
}
