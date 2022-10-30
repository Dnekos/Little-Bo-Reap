using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates
{
	WANDER = 0,
	CHASE_PLAYER = 1,
	HITSTUN = 2,
	IDLE = 3,
	EXECUTABLE = 4
}

public class EnemyAI : Damageable
{
	[Header("Spawning")]
	public GameObject SpawnParticlePrefab;
	public float SpawnWaitTime = 2;

	protected Coroutine QueuedAttack = null;

	[Header("Enemy State")]
	[SerializeField] protected EnemyStates currentEnemyState;
	[SerializeField] float StunTime = 0.3f;
	[SerializeField] LayerMask playerLayer;

	[Header("Execution Variables")]
	public bool isExecutable;
	public bool mustBeExecuted;
	[SerializeField] protected int executionHealthThreshhold;
	[SerializeField] protected GameObject executeTrigger;
	public Transform executePlayerPos;
	public Execution execution;

	EnemyStates stunState;

	[Header("Ground Check")]
	[SerializeField] LayerMask groundLayer;
	[SerializeField] Transform groundCheckOriginFront;
	[SerializeField] Transform groundCheckOriginBack;
	[SerializeField] float groundCheckDistance;
	public bool isGrounded = false;
	[SerializeField] float fallRate = 50;

	protected Transform player;
	NavMeshAgent agent;

    // Start is called before the first frame update
    override protected void Start()
    {
		base.Start();
		agent = GetComponent<NavMeshAgent>();
		player = GameManager.Instance.GetPlayer();
	}

	// Update is called once per frame
	protected virtual void Update()
    {
		if (WorldState.instance.Dead)
		{
			DoIdle();
		}


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
			case EnemyStates.EXECUTABLE:
				DoExecutionState();
				break;
			default:
				Debug.LogWarning("Enemy at unexpected state and defaulted!");
				break;
		}
	}

	public EnemyStates GetState()
    {
		return currentEnemyState;
    }

	void DoIdle()
    {
		//debug idle state TODO: maybe make this not every frame?
		if (Vector3.Distance(transform.position,player.position) <= 20)
			currentEnemyState = EnemyStates.CHASE_PLAYER;
    }

	#region Execution State

	void DoExecutionState()
    {

    }

	public void Execute()
    {
		base.OnDeath();
	}

    #endregion

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
			//rb.isKinematic = true;
			agent.enabled = true;


			//freeze
			rb.constraints = RigidbodyConstraints.FreezeAll;

			rb.velocity = Vector3.zero;
			rb.angularVelocity = Vector3.zero;
		}
	}
	#endregion

	#region Health Override and Hitstun
	//to apply normal damage, use this overload
	public override void TakeDamage(Attack atk, Vector3 attackForward)
	{
		//if they must be executed, return
		if (mustBeExecuted && Health < executionHealthThreshhold)
			return;

		// give them hitstun
		if (atk.DealsHitstun)
		{
			//StopCoroutine("OnHitStun");
			StopAllCoroutines();
			StartCoroutine("OnHitStun");
		}

		// subtract health
		base.TakeDamage(atk, attackForward);

		if (isExecutable && Health <= executionHealthThreshhold)
		{
			rb.mass = 100f;
			rb.velocity = Vector3.zero;
			agent.enabled = false;
			gameObject.layer = LayerMask.NameToLayer("EnemyExecute");
			rb.constraints = RigidbodyConstraints.FreezeAll;
			currentEnemyState = EnemyStates.EXECUTABLE;
			executeTrigger.SetActive(true);
		}
	}
	protected override void OnDeath()
	{
		if (isExecutable)
		{
			rb.mass = 100f;
			rb.velocity = Vector3.zero;
			agent.enabled = false;
			gameObject.layer = LayerMask.NameToLayer("EnemyExecute");
			rb.constraints = RigidbodyConstraints.FreezeAll;
			currentEnemyState = EnemyStates.EXECUTABLE;
			executeTrigger.SetActive(true);
		}
		else
			base.OnDeath();
	}
    public override void ForceKill()
    {
		isExecutable = false;
        base.ForceKill();
    }

    //to apply black sheep damage, use this overload
    public override void TakeDamage(SheepAttack atk, Vector3 attackForward)
	{
		//if they must be executed, return
		if (mustBeExecuted && Health < executionHealthThreshhold)
			return;

		// give them hitstun
		if (atk.dealsHitstunBlack)
		{
			//StopCoroutine("OnHitStun");
			StopAllCoroutines();
			StartCoroutine("OnHitStun");
		}

		// subtract health
		base.TakeDamage(atk, attackForward);

		if (Health <= executionHealthThreshhold && isExecutable)
		{
			rb.mass = 100f;
			rb.velocity = Vector3.zero;
			agent.enabled = false;
			gameObject.layer = LayerMask.NameToLayer("EnemyExecute");
			rb.constraints = RigidbodyConstraints.FreezeAll;
			currentEnemyState = EnemyStates.EXECUTABLE;
			executeTrigger.gameObject.SetActive(true);
		}
	}


	IEnumerator OnHitStun()
	{
		// save current state and set to Hitstun
		stunState = (currentEnemyState == EnemyStates.HITSTUN) ? stunState : currentEnemyState;
		currentEnemyState = EnemyStates.HITSTUN;

		//turn on rb and turn off navmesh (turned on in GroundCheck (which cant be called when hitstunned))
		//rb.isKinematic = false;
		agent.enabled = false;
		rb.constraints = RigidbodyConstraints.None;
		rb.constraints = RigidbodyConstraints.FreezeRotation;

		yield return new WaitForSeconds(StunTime);

		// stay in stun until touching the ground
		do
		{
			yield return new WaitForSeconds(0.01f);
			GroundCheck();
		} while (!isGrounded);

		//reset if not in execute stage
		//Demetri this is a quick n dirty fix might need to move around execute stuff eventually
		if (currentEnemyState != EnemyStates.EXECUTABLE) currentEnemyState = stunState;

		//freeze dammit
		rb.constraints = RigidbodyConstraints.FreezeAll;

		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
	}
	#endregion
}