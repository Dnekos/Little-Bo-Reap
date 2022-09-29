using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

enum EnemyStates
{
	WANDER = 0,
	CHASE_PLAYER = 1,
	HITSTUN = 2,
	IDLE = 3
}

public class EnemyAI : Damageable
{
	[Header("Attacking")]
	[SerializeField] Attack StickAttack;
	[SerializeField] List<Transform> NearbyGuys;
	[SerializeField] Collider StickCollider;
	Coroutine QueuedAttack = null;
	Animator anim;

	[Header("Shockwave")]
	[SerializeField] GameObject ShockwavePrefab;
	[SerializeField] Transform ShockwaveSpawnPoint;

	[Header("Enemy State")]
	[SerializeField] EnemyStates currentEnemyState;

	[SerializeField] float StunTime = 0.3f;
	[SerializeField] LayerMask playerLayer;
	EnemyStates stunState;

	[Header("Ground Check")]
	[SerializeField] LayerMask groundLayer;
	[SerializeField] Transform groundCheckOriginFront;
	[SerializeField] Transform groundCheckOriginBack;
	[SerializeField] float groundCheckDistance;
	public bool isGrounded = false;

	Transform player;
	NavMeshAgent agent;

    // Start is called before the first frame update
    override protected void Start()
    {
		base.Start();
		agent = GetComponent<NavMeshAgent>();
		player = GameManager.Instance.GetPlayer();
		NearbyGuys = new List<Transform>();
		anim = GetComponent<Animator>();
	}

	// Update is called once per frame
	void Update()
    {
		// basic state machine
		switch (currentEnemyState)
		{
			case EnemyStates.WANDER:
				break;
			case EnemyStates.CHASE_PLAYER:
				DoChase();
				break;
			case EnemyStates.HITSTUN:
				break;
			case EnemyStates.IDLE:
				DoIdle();
				break;
			default:
				Debug.LogWarning("Enemy at unexpected state and defaulted!");
				break;
		}
	}

	void DoIdle()
    {
		//debug idle state TODO: maybe make this not every frame?
		if (Vector3.Distance(transform.position,player.position) <= 20)
			currentEnemyState = EnemyStates.CHASE_PLAYER;
    }

	#region Chasing and Attacking
	void DoChase()
	{
		GroundCheck();
		if (isGrounded)
			agent.SetDestination(player.position);

		// if Coroutine is not running, run it
		QueuedAttack ??= StartCoroutine(AttackCheck());
	}
	IEnumerator AttackCheck()
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

			if (NearbyGuys.Count != 0 && angle < 60)
				anim.SetTrigger("Attack1");
			else
				anim.SetTrigger("Attack2");
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
			collision.gameObject.GetComponent<Damageable>()?.TakeDamage(StickAttack, transform.forward);
		}
	}
	#endregion

	#region Movement
	void GroundCheck()
	{
		// shamelessly stolen from playermovement TODO: combine groundchecks
		bool frontCheck = false;
		bool backCheck = false;
		Vector3 frontNormal;
		Vector3 backNormal;

		//set ground check
		RaycastHit hitFront;
		frontCheck = Physics.Raycast(groundCheckOriginFront.position, Vector3.down, out hitFront, groundCheckDistance, groundLayer);
		//if canJump, groundNormal = hit.normal, else groudnnormal = vector3.up  v ternary operater
		frontNormal = frontCheck ? hitFront.normal : Vector3.up;

		RaycastHit hitBack;
		backCheck = Physics.Raycast(groundCheckOriginBack.position, Vector3.down, out hitBack, groundCheckDistance, groundLayer);
		backNormal = backCheck ? hitBack.normal : Vector3.up;

		isGrounded = frontCheck || backCheck;

		if (!agent.enabled && isGrounded)
		{
			rb.isKinematic = true;
			agent.enabled = true;
		}
	}
	#endregion

	#region Health Override and Hitstun
	public override void TakeDamage(Attack atk, Vector3 attackForward)
	{
		// give them hitstun
		if (atk.DealsHitstun)
		{
			StopCoroutine("OnHitStun");
			StartCoroutine("OnHitStun");
		}

		// subtract health
		base.TakeDamage(atk, attackForward);
	}
	IEnumerator OnHitStun()
	{
		// save current state and set to Hitstun
		stunState = (currentEnemyState == EnemyStates.HITSTUN) ? stunState : currentEnemyState;
		currentEnemyState = EnemyStates.HITSTUN;

		//turn on rb and turn off navmesh (turned on in GroundCheck (which cant be called when hitstunned))
		rb.isKinematic = false;
		agent.enabled = false;

		yield return new WaitForSeconds(StunTime);

		currentEnemyState = stunState;
	}
	#endregion
}