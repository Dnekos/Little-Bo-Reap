using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Attack", menuName = "ScriptableObjects/Enemy Attack", order = 2)]
public class EnemyAttack : Attack
{

	[Header("SpawningHitbox")]
	public GameObject hitboxPrefab;

	[Header("Cooldown")]
	[Tooltip("Enemy will be unable to use this attack while coolingdown")]
	public float MaxCooldown = 3;

	[Tooltip("keep this 4 characters pls")]
	public string ID;

	//	public virtual void SpawnObject(Vector3 pos)
	public virtual void SpawnObject(Transform t)
	{
		if (hitboxPrefab != null)
		{
			Instantiate(hitboxPrefab, t.position, t.rotation);
		}
	}

	public virtual bool CheckCondition(Transform user, Transform player, List<Transform> NearbyGuys)
	{
		return true;
	}
	public virtual void PerformAttack(Animator anim)
	{
		anim.Play(animation);

	}
}
