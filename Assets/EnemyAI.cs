using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

enum EnemyStates
{
	WANDER = 0,
	CHASE_PLAYER = 1,
	HITSTUN = 2
}

public class EnemyAI : Damageable
{
	[Header("Enemy State")]
	[SerializeField] EnemyStates currentEnemyState;

	[SerializeField] float StunTime = 0.3f;
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
			default:
				Debug.LogWarning("Enemy at unexpected state and defaulted!");
				break;
		}
	}


	void DoChase()
	{
		GroundCheck();
		if (isGrounded)
			agent.SetDestination(player.position);
	}

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

	#region Damage and HitStun
	public override void TakeDamage(Attack atk, Vector3 attackForward)
	{
		// give them hitstun
		StopCoroutine("OnHitStun");
		StartCoroutine("OnHitStun");

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