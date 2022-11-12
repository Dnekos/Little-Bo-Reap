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
	[System.Serializable]
	protected struct AttackCooldownCombo
	{
		public EnemyAttack atk;
		public float Cooldown;
	}

	[Header("Spawning")]
	public GameObject SpawnParticlePrefab;
	public float SpawnWaitTime = 2;

	protected Coroutine QueuedAttack = null;

	[Header("Enemy State")]
	[SerializeField] protected EnemyStates currentEnemyState;
	[SerializeField] float StunTime = 0.3f;
	[SerializeField] LayerMask playerLayer;

	[Header("Attacking")]
	[SerializeField,Tooltip("priority of attacks are based on how high up they are in the array")] protected AttackCooldownCombo[] attacks;
	protected int activeAttackIndex = -1;
	[SerializeField] List<Transform> NearbyGuys;
	[SerializeField, Tooltip("how frequently enemies query conditions to make an attack")] float delayBetweenAttacks = 1;
	[SerializeField] int SheepToDistract = 2;
	[SerializeField] Collider StickCollider;
	[SerializeField] Animator anim;

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

	[Header("Sounds")]
	[SerializeField] FMODUnity.EventReference swingSound;
	[SerializeField] FMODUnity.EventReference clubHitSound;

	protected Transform player;
	NavMeshAgent agent;

    // Start is called before the first frame update
    override protected void Start()
    {
		base.Start();
		NearbyGuys = new List<Transform>();
		agent = GetComponent<NavMeshAgent>();
		player = GameManager.Instance.GetPlayer();
	}

	// Update is called once per frame
	protected virtual void Update()
    {
		if(GetComponent<Animator>()!=null)
        {
		if(agent.velocity.magnitude > 1)
        {
			GetComponent<Animator>()?.SetBool("isMoving", true);
        }
		else
        {
			GetComponent<Animator>()?.SetBool("isMoving", false);
		}
        }
		

		if (WorldState.instance.Dead)
		{
			DoIdle();
		}

		// update cooldowns on attacks
		for (int i = 0; i < attacks.Length; i++)
		{
			attacks[i].Cooldown -= Time.deltaTime;
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

	#region UtilityFunctions
	public EnemyStates GetState()
    {
		return currentEnemyState;
    }
	protected void EnemySetDestination(Vector3 dest)
	{
		if (!agent.isOnNavMesh && !agent.isOnOffMeshLink)
		{
			print(gameObject + " failed to find a destination");
			base.OnDeath();
		}
		else
			agent.SetDestination(dest);
	}
	#endregion

	void DoIdle()
    {
		//debug idle state TODO: maybe make this not every frame?
		if (Vector3.Distance(transform.position,player.position) <= 20)
			currentEnemyState = EnemyStates.CHASE_PLAYER;
    }

	public void ToChase()
	{
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
		// double check that there are no null sheep (possibly could happen if they are killed in the radius)
		NearbyGuys.RemoveAll(item => item == null);

		// if there are sheep near it, follow them instead
		if (NearbyGuys.Count >= SheepToDistract)
			EnemySetDestination(ClosestGuy());
		else
			EnemySetDestination(player.position);

		// if Coroutine is not running, run it
		QueuedAttack ??= StartCoroutine(AttackCheck());
	}
	Vector3 ClosestGuy()
	{
		float dist = Mathf.Infinity;
		Vector3 closestPos = NearbyGuys[0].position;
		for (int i = 0; i< NearbyGuys.Count; i++)
		{
			float newdist = (NearbyGuys[i].position - transform.position).sqrMagnitude;
			if (newdist < dist)
			{
				dist = newdist;
				closestPos = NearbyGuys[i].position;
			}
		}
		return closestPos;
	}
	virtual protected IEnumerator AttackCheck()
	{
		yield return new WaitForSeconds(delayBetweenAttacks);
		if (currentEnemyState == EnemyStates.CHASE_PLAYER)
		{

			for (int i = 0; i < attacks.Length; i++)
			{
				if (attacks[i].Cooldown < 0 && attacks[i].atk.CheckCondition(transform, player, NearbyGuys))
				{
					attacks[i].atk.PerformAttack(anim);
					attacks[i].Cooldown = attacks[i].atk.MaxCooldown;
					activeAttackIndex = i;
					break;
				}
			}
		}
		QueuedAttack = null;

	}
	#endregion

	#region Collisions
	private void OnCollisionEnter(Collision collision)
	{
		// double check that the collision is due to the attack
		if (collision.GetContact(0).thisCollider == StickCollider)
		{
			Debug.Log("collided with " + collision.gameObject.name);
			Damageable hitTarget = collision.gameObject.GetComponent<Damageable>();
			if (hitTarget != null)
			{
				collision.gameObject.GetComponent<Damageable>()?.TakeDamage(attacks[activeAttackIndex].atk, transform.forward);
				FMODUnity.RuntimeManager.PlayOneShotAttached(clubHitSound, gameObject);
			}

		}
	}

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

		Debug.Log(isGrounded);

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