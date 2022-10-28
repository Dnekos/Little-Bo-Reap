using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class BigGuyAI : EnemyAI
{
	[Header("HealthBar")]
	[SerializeField] GameObject HealthBarCanvas;
	[SerializeField] Transform[] HPBars;

	[Header("Attacking")]
	[SerializeField] Attack StickAttack;
	[SerializeField] List<Transform> NearbyGuys;
	[SerializeField] Collider StickCollider;
	Animator anim;

	[Header("Shockwave")]
	[SerializeField] Attack ShockwaveAttack;
	[SerializeField] GameObject ShockwavePrefab;
	[SerializeField] Transform ShockwaveSpawnPoint;

	[Header("Sounds")]
	[SerializeField] FMODUnity.EventReference swingSound;
	[SerializeField] FMODUnity.EventReference clubHitSound;

	// Start is called before the first frame update
	override protected void Start()
    {
		base.Start();
		NearbyGuys = new List<Transform>();
		anim = GetComponent<Animator>();
		HealthBarCanvas.SetActive(false);
	}

	#region Chasing and Attacking
	protected override IEnumerator AttackCheck()
	{
		yield return new WaitForSeconds(3);
		if (currentEnemyState == EnemyStates.CHASE_PLAYER)
		{
			// double check that there are no null sheep (possibly could happen if they are killed in the radius)
			NearbyGuys.RemoveAll(item => item == null);

			Vector3 average_pos = Vector3.zero;
			for (int i = 0; i < NearbyGuys.Count; i++)
			{
				average_pos += NearbyGuys[i].position;
			}

			// check to see if most of the sheep are in front of him TODO, make this more modular
			Vector3 heading = (average_pos / NearbyGuys.Count) - transform.position;
			float angle = Vector3.Angle(transform.forward, heading);

			// if there are sheep in front
			if (NearbyGuys.Count != 0 && angle < 90)
			{
				FMODUnity.RuntimeManager.PlayOneShotAttached(swingSound, gameObject);
				anim.Play(StickAttack.animation);
			}
			// if at least half of active sheep are around it
			else if (NearbyGuys.Count > player.GetComponent<PlayerSheepAbilities>().GetAverageActiveFlockSize() * 0.5f)
			{
				FMODUnity.RuntimeManager.PlayOneShotAttached(swingSound, gameObject);
				anim.Play(ShockwaveAttack.animation);
			}
		}
		QueuedAttack = null;

	}
	#endregion

	#region Shockwave (Stomp) Attack
	private void OnTriggerEnter(Collider other)
	{
		if (other.GetComponent<PlayerSheepAI>() != null || other.GetComponent<PlayerMovement>() != null)
		{
			NearbyGuys.Add(other.transform);
		}
	}
	private void OnTriggerExit(Collider other)
	{
		if (other.GetComponent<PlayerSheepAI>() != null || other.GetComponent<PlayerMovement>() != null)
		{
			NearbyGuys.Remove(other.transform);
		}

	}

	public void SpawnShockwave()
	{
		Instantiate(ShockwavePrefab, ShockwaveSpawnPoint.position, new Quaternion());
	}
	#endregion

	#region Stick Collision
	private void OnCollisionEnter(Collision collision)
	{
		// double check that the collision is due to the attack
		if (collision.GetContact(0).thisCollider == StickCollider)
		{
			Debug.Log("collided with " + collision.gameObject.name);
			Damageable hitTarget = collision.gameObject.GetComponent<Damageable>();
			if (hitTarget != null)
			{
				collision.gameObject.GetComponent<Damageable>()?.TakeDamage(StickAttack, transform.forward);
				FMODUnity.RuntimeManager.PlayOneShotAttached(clubHitSound, gameObject);
			}

		}
	}
	#endregion


	#region Healthbar
	protected override void Update()
	{
		base.Update();

		// if healthbar is active, billboard it
		if (Health != MaxHealth)
			HealthBarCanvas.transform.LookAt(Camera.main.transform);
	}

	public override void TakeDamage(Attack atk, Vector3 attackForward)
	{
		base.TakeDamage(atk, attackForward);

		// when taking damage, open healthbar
		if (Health != MaxHealth)
		{
			HealthBarCanvas.SetActive(true);
			float healthbarScale = 1 - (Health / MaxHealth);
			HPBars[0].localScale = new Vector3(healthbarScale, 1, 1);
			HPBars[1].localScale = new Vector3(healthbarScale * -1, 1, 1);
		}
		else
			HealthBarCanvas.SetActive(false);

	}
	#endregion
}