using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum SheepStates
{
    FOLLOW_PLAYER = 0,
    WANDER = 1,
    CHARGE = 2,
    DEFEND_PLAYER = 3,
	CONSTRUCT = 4,
    ATTACK = 5,
	STUN = 6, // TODO, make this the same as the enemy's
	LIFT
}

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerSheepAI : Damageable
{
    [Header("Sheep State Variables")]
    [SerializeField] SheepTypes sheepType;
    [SerializeField] SheepStates currentSheepState;
    [SerializeField] float baseSpeedMin = 15f;
    [SerializeField] float baseSpeedMax = 20f;
    float baseSpeedCurrent;
    float agentStoppingDistance;
    Transform player;
    NavMeshAgent agent;
    Animator animator;

	[Header("Follow State Variables")]
    [SerializeField] float avoidPlayerDistance;
    [SerializeField] float avoidPlayerSpeed = 40f;
    [SerializeField] float followStoppingDistanceMin = 5f;
    [SerializeField] float followStoppingDistanceMax = 10f;

    [Header("Leader Variables")]
    [SerializeField] GameObject leaderIndicator;
    public PlayerSheepAI leaderSheep;
    public bool isLeader;

    [Header("Black Sheep Variables")]
    public bool isBlackSheep = false;
    [SerializeField] GameObject blackSheepParticles;
    public Attack selfDamage;

    [Header("Wander State Variables")]
    [SerializeField] float wanderSpeed = 10f;
    [SerializeField] float wanderRadius;
    [SerializeField] float wanderStopDistance;
    [SerializeField] float wanderDelayMin = 1f;
    [SerializeField] float wanderDelayMax = 3f;
    bool canWander = true;

    [Header("Attack State Variables")]
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] float attackDetectionRadius;
    [SerializeField] EnemyAI attackTargetCurrent;
    [SerializeField] List<EnemyAI> attackTargets;
    [SerializeField] string attackAnimation;
    [SerializeField] float attackCooldown = 1.5f;
    [SerializeField] float distanceToAttack;
	[SerializeField] float attackStopDistance;

	public SheepAttack attackBase;
    //public float attackDamage = 5f;
    [SerializeField] bool canAttack = true;

    [Header("Charge State Variables")]
    [SerializeField] float chargeSpeed = 35f;
    [SerializeField] float chargePointRadius = 10f;
    [SerializeField] float chargeStopDistance = 0f;
    [SerializeField] float chargeEndDistance = 1f;
    [SerializeField] float chargeCheckTime = 1f;
    [SerializeField] float chargeCheckSpeed = 2f;
    [SerializeField] SheepAttack chargeAttack;
    [SerializeField] Collider chargeCollider;
    float chargeCheckCurrent = 0;
    Vector3 chargePoint;
    bool isCharging;

    [Header("Defend State Variables")]
    [SerializeField] float defendSpeed = 35f;
    [SerializeField] float defendStopDistance = 0f;
    [SerializeField] SheepAttack defendAttack;
    Transform defendPoint;

	[Header("Stun State Variables")]
	[SerializeField] float StunTime = 1;
	[SerializeField] float fallRate = 50;
	bool isGrounded;
	SheepHolder owningConstruct;

    override protected void Start()
    {
		base.Start();

        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
		baseSpeedCurrent = GetRandomSheepBaseSpeed();
        player = GameManager.Instance.GetPlayer();

        //get random follow stopping distance
        //this prevents sheep from clumping up and getting jittery when in a flock behind player
        agentStoppingDistance = Random.Range(followStoppingDistanceMin, followStoppingDistanceMax);


        FindLeader();

        //check black sheep stuff
        if (isBlackSheep) blackSheepParticles.SetActive(true);
    }
    void FindLeader()
    {
        //if leader exists, assign 
        if (player.GetComponent<PlayerSheepAbilities>().leaderSheep[((int)sheepType)] != null)
        {
            leaderSheep = player.GetComponent<PlayerSheepAbilities>().leaderSheep[((int)sheepType)];
        }
        //if no leader, congrats, ur the leader
        else
        {
            isLeader = true;
            player.GetComponent<PlayerSheepAbilities>().leaderSheep[((int)sheepType)] = this;
            leaderIndicator.SetActive(true);
        }
    }
    void CheckLeader()
    {
        if (leaderSheep == null) FindLeader();
    }
    void CheckAnimation()
    {
        if (agent.velocity.magnitude > 1) animator.SetBool("isMoving", true);
        else animator.SetBool("isMoving", false);
    }

    public void GothMode()
    {
        isBlackSheep = true;
        blackSheepParticles.SetActive(true);
    }

    private void Update()
    {

        CheckAnimation();
        CheckLeader();

        //state machine
        switch(currentSheepState)
        {
            case SheepStates.FOLLOW_PLAYER:
                {
                    DoFollowPlayer();
                    break;
                }
            case SheepStates.WANDER:
                {
                    DoWander();
                    break;
                }
            case SheepStates.CHARGE:
                {
                    DoCharge();
                    CheckCharge();
                    break;
                }
            case SheepStates.DEFEND_PLAYER:
                {
                    DoDefendPlayer();
                    break;
                }
			case SheepStates.CONSTRUCT:
                {
                    break;
                }
            case SheepStates.ATTACK:
                {
                    DoAttack();
                    break;
                }
            case SheepStates.STUN:
                {
					if (!isGrounded)
						rb.AddForce(Vector3.down * fallRate);
					break;
                }
			case SheepStates.LIFT:
				{
					break;
				}
			default:
                {
                    Debug.LogWarning("PlayerSheepAI should never default!");
                    break;
                }
        }
    }

    void DealDamage(Collider target, SheepAttack theAttack, bool blackSheepDamage)
    {
        if(blackSheepDamage)
        {
            //subtract 1 from health
            TakeDamage(selfDamage, transform.forward);

            Instantiate(theAttack.explosionEffect, transform.position, transform.rotation);
            target?.GetComponent<EnemyAI>().TakeDamage(theAttack, transform.forward);
        }
        else target?.GetComponent<EnemyAI>().TakeDamage((Attack)theAttack, transform.forward);
    }
    private void OnTriggerEnter(Collider other)
    {
        switch (currentSheepState)
        {
            case SheepStates.CHARGE:
                {
                    if (other.CompareTag("Enemy"))
                    {
                        DealDamage(other, chargeAttack, isBlackSheep);
                    }
                    break;
                }
            case SheepStates.DEFEND_PLAYER:
                {
                    if (other.CompareTag("Enemy"))
                    {
                        DealDamage(other, defendAttack, isBlackSheep);
                        TakeDamage(selfDamage, transform.forward);
                    }
                    break;
                }
			case SheepStates.STUN:
				break;	
			default:
                {
                    break;
                }
        }

    }

    #region Utility Functions
    //ok so due to some bullshit you CANNOT remove sheep from list in a for loop? so use this and clear list after for resummoning sheep
    public void DestroySheep()
    {
        Destroy(gameObject);
    }
    //this is called to kill an indvidual sheep and remove it from list
    public void KillSheep()
    {
        player.GetComponent<PlayerSheepAbilities>().RemoveSheepFromList(sheepType, this);
        DestroySheep();
    }
    public void RecallSheep()
    {
		// sheep cant be recalled when stunned
		if (currentSheepState == SheepStates.STUN)
			return;

        currentSheepState = SheepStates.FOLLOW_PLAYER;

		EndConstruct();
	}
	public SheepStates GetSheepState()
    {
        return currentSheepState;
    }
	public bool IsCommandable()
	{
		return currentSheepState == SheepStates.CHARGE || currentSheepState == SheepStates.DEFEND_PLAYER || currentSheepState == SheepStates.FOLLOW_PLAYER;
	}
    float GetRandomSheepBaseSpeed()
    {
        float speed = Random.Range(baseSpeedMin, baseSpeedMax);
        return speed;
    }
	#endregion

	#region Health
	protected override void OnDeath()
	{
		KillSheep();
	}
	public override void TakeDamage(Attack atk, Vector3 attackForward)
	{
		if (atk.DealsHitstun)
		{
			StartCoroutine(OnHitStun(SheepStates.WANDER));
		}

		base.TakeDamage(atk, attackForward);
	}
	#endregion

	#region Follow Player
	void DoFollowPlayer()
    {
        //if player is too close, part the red sea!
        float checkDistance = Vector3.Distance(transform.position, player.position);

        if (checkDistance < avoidPlayerDistance)
        {
            //get direction of player
            Vector3 dir = transform.position - player.position;

            //set destination
            Vector3 avoidDestination = transform.position + dir;

            //move away
            agent.speed = avoidPlayerSpeed;
            agent.stoppingDistance = 0;
            agent.SetDestination(avoidDestination);

            return;
        }
        else
        {
            //set speed and follow distance
            agent.speed = baseSpeedCurrent;
            agent.stoppingDistance = agentStoppingDistance;
			agent.SetDestination(player.position);
		}

    }
    #endregion

    #region Sheep Stun Demetri Name your regions idiot +2 baby points
    IEnumerator OnHitStun(SheepStates stateAfterStun)
	{
		// save current state and set to Hitstun
		currentSheepState = SheepStates.STUN;
		gameObject.layer = LayerMask.NameToLayer("PlayerSheep");
		//turn on rb and turn off navmesh (turned on in GroundCheck (which cant be called when hitstunned))
		//rb.isKinematic = false;
		agent.enabled = false;
		isGrounded = false;

		rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.None;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        yield return new WaitForSeconds(StunTime);

		// wait until grounded
		yield return new WaitUntil(() => isGrounded);
			//yield return new WaitForSeconds(0.1f);

		currentSheepState = stateAfterStun;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (currentSheepState == SheepStates.STUN && collision.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
			isGrounded = true;
            //rb.isKinematic = true;
            agent.enabled = true;

            rb.constraints = RigidbodyConstraints.FreezeAll;

            rb.angularVelocity = Vector3.zero;
            rb.velocity = Vector3.zero;
        }
	}
	#endregion

	#region Wander
    void GoWandering()
    {
        //stop wander call
        canWander = false;

        //get random point inside radius
        Vector3 destination = Vector3.zero;
        Vector3 randomPosition = Random.insideUnitSphere * wanderRadius;
        randomPosition += transform.position;

        //if inside navmesh, charge!
        if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, wanderRadius, 1))
        {
            //get charge
            destination = hit.position;

            //set agent destination
            agent.destination = destination;
        }

        //wander cooldown
        StartCoroutine(WanderCooldown());
    }

	void DoWander()
    {
        //if stopped, pick new point to wander!
        if (Vector3.Distance(transform.position, agent.destination) <= 1f && canWander)
        {
            GoWandering();
        }


        //then, check if there are enemies nearby
        //Demetri I am using Physics.CheckSphere against your wishes
        if(Physics.CheckSphere(transform.position, attackDetectionRadius, enemyLayer))
        {
            FindAttackTargets();
            currentSheepState = SheepStates.ATTACK;
			agent.stoppingDistance = attackStopDistance;
            agent.speed = baseSpeedCurrent;
        }
    }
    IEnumerator WanderCooldown()
    {
        float randTime = Random.Range(wanderDelayMin, wanderDelayMax);
        yield return new WaitForSeconds(randTime);
        canWander = true;
    }
    #endregion

    #region Attack
    void DoAttack()
    {  
        if(attackTargetCurrent!= null)
			agent.SetDestination(attackTargetCurrent.transform.position);


        if (canAttack)
        {
            //first check if we have a target and are in range
            if (attackTargetCurrent != null && Vector3.Distance(transform.position, attackTargetCurrent.transform.position) <= distanceToAttack)
            {
                //if the target is executable, remove them from the list
                if (attackTargetCurrent.GetState() == EnemyStates.EXECUTABLE) attackTargetCurrent = null;

                else
                {
                    agent.SetDestination(transform.position);
                    transform.LookAt(attackTargetCurrent.transform);
                    animator.Play(attackAnimation);
                    canAttack = false;
                    StartCoroutine(AttackCooldown());
                }
            }
            //if no target, go to next in list or find one!
            else if (attackTargetCurrent == null)
            {
                attackTargetCurrent = GetAttackTarget();

                //still no target? then go back to wander state
                if (attackTargetCurrent == null)
                {
                    Debug.Log("Attack target is null, going to wander");
                    agent.SetDestination(transform.position);
                    agent.speed = wanderSpeed;
                    agent.stoppingDistance = wanderStopDistance;
                    currentSheepState = SheepStates.WANDER;

                    GoWandering();
                }
            }
        }

    }
    EnemyAI GetAttackTarget()
    {
        //find targets to attack
        FindAttackTargets();

        if(attackTargets.Count > 0)
        {
            //return a random one
            int rand = Random.Range(0, attackTargets.Count);
            return attackTargets[rand];
        }
        else return null;
    }

    void FindAttackTargets()
    {
        attackTargets.Clear();

        //check if there are enemies nearby
        //Demetri I am using Physics.OverlapSphere against your wishes
        Collider[] enemyHits = (Physics.OverlapSphere(transform.position, attackDetectionRadius, enemyLayer));
        foreach(Collider enemy in enemyHits)
        {
            if (enemy.GetComponent<EnemyAI>() !=null && enemy.GetComponent<EnemyAI>().GetState() != EnemyStates.EXECUTABLE) attackTargets?.Add(enemy.GetComponent<EnemyAI>());
        }
    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    #endregion

    #region Charge
    void DoCharge()
    {
        //if agent reaches charge point, go into wander mode
        if(Vector3.Distance(transform.position, agent.destination) <= chargeEndDistance)
        {
            isCharging = false;
            agent.speed = wanderSpeed;
            agent.stoppingDistance = wanderStopDistance;
            currentSheepState = SheepStates.WANDER;
        }
    }

    //check here to make sure our sheep arent stuck in charge or caught on something.
    void CheckCharge()
    {
        chargeCheckCurrent += Time.deltaTime;

        //if time is past threshold and our movement velocity is too low, end charge early.
        if(chargeCheckCurrent > chargeCheckTime && agent.velocity.magnitude <= chargeCheckSpeed)
        {
            isCharging = false;
            agent.speed = wanderSpeed;
            agent.stoppingDistance = wanderStopDistance;
            currentSheepState = SheepStates.WANDER;
        }
    }

    public void BeginCharge(Vector3 theChargePosition)
    {
        //CHARGE!
        isCharging = true;

        //set timer to 0
        chargeCheckCurrent = 0;

        //set destination
        //get random point inside radius
        Vector3 chargePosition = theChargePosition;
        Vector3 randomPosition = Random.insideUnitSphere * chargePointRadius;
        randomPosition += theChargePosition;

        //if inside navmesh, charge!
        if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, chargePointRadius, 1))
        {
            //get charge
            chargePosition = hit.position;

            //set agent destination
            agent.destination = chargePosition;
        }
        else
        {
            agent.destination = chargePosition;
        }

        //set speed
        agent.speed = chargeSpeed;
        agent.stoppingDistance = chargeStopDistance;

        //set sheep state
        currentSheepState = SheepStates.CHARGE;
    }
    #endregion

    #region Defend Player
    void DoDefendPlayer()
    {
        agent.SetDestination(defendPoint.position);
    }
    public void BeginDefendPlayer(Transform theDefendPoint)
    {
        //set defened mode
        defendPoint = theDefendPoint;

        //set speed
        agent.speed = defendSpeed;
        agent.stoppingDistance = defendStopDistance;

        currentSheepState = SheepStates.DEFEND_PLAYER;
    }
	#endregion

	#region Sheep Construct / Lift
	public void StartLift()
	{
		currentSheepState = SheepStates.LIFT;
		agent.enabled = false;
		rb.isKinematic = true;
		gameObject.layer = LayerMask.NameToLayer("Ground");
	}
	public void CancelLift()
	{
		currentSheepState = SheepStates.FOLLOW_PLAYER;
		agent.enabled = true;

		rb.constraints = RigidbodyConstraints.FreezeAll;

		rb.angularVelocity = Vector3.zero;
		rb.velocity = Vector3.zero;
	}
	public void EndLift(bool kill)
	{
		if (kill)
			KillSheep();
		else
			StartCoroutine(OnHitStun(SheepStates.FOLLOW_PLAYER));
	}

	public void DoConstruct(SheepHolder cons)
	{
		owningConstruct = cons;
		currentSheepState = SheepStates.CONSTRUCT;
		agent.enabled = false;
	}
	public void EndConstruct()
	{
		agent.enabled = true;
		if (owningConstruct != null)
		{
			owningConstruct.RemoveSheep(transform);
			owningConstruct = null;
		}

		// if not already changed, make sure its not on CONSTRUCT
		if (currentSheepState == SheepStates.CONSTRUCT)
			StartCoroutine(OnHitStun(SheepStates.WANDER));
	}
	#endregion
}
