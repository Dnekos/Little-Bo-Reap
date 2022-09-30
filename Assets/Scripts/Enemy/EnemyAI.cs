using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates
{
	WANDER = 0,
	CHASE_PLAYER = 1,
	HITSTUN = 2,
	IDLE = 3
}

public class EnemyAI : Damageable
{
	protected Coroutine QueuedAttack = null;

	[Header("Enemy State")]
	[SerializeField] protected EnemyStates currentEnemyState;
	[SerializeField] float StunTime = 0.3f;
	[SerializeField] LayerMask playerLayer;

	EnemyStates stunState;

	[Header("Ground Check")]
	[SerializeField] LayerMask groundLayer;
	[SerializeField] Transform groundCheckOriginFront;
	[SerializeField] Transform groundCheckOriginBack;
	[SerializeField] float groundCheckDistance;
	public bool isGrounded = false;
	[SerializeField] float fallRate = 50;

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
				//apply gravity if falling
				if (!isGrounded)
					rb.AddForce(Vector3.down * fallRate);

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
		//if (isGrounded)
			agent.SetDestination(player.position);

		// if Coroutine is not running, run it
		QueuedAttack ??= StartCoroutine(AttackCheck());
	}
	virtual protected IEnumerator AttackCheck()
	{
		yield return new WaitForSeconds(0);
		
		QueuedAttack = null;
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

		// stay in stun until touching the ground
		do
		{
			yield return new WaitForSeconds(0.01f);
			GroundCheck();
		} while (!isGrounded);

		currentEnemyState = stunState;
	}
	#endregion
}