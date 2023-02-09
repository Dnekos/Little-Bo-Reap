using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum SheepStates
{
    FOLLOW_PLAYER = 0,
    WANDER = 1,
    STAMPEDE = 2,
    VORTEX = 3,
	CONSTRUCT = 4,
    ATTACK = 5,
	STUN = 6, // TODO, make this the same as the enemy's
	LIFT
}

[RequireComponent(typeof(NavMeshAgent))]
 public partial class PlayerSheepAI : Damageable
{
    [Header("Sheep State Variables")]
    [SerializeField] int sheepType;
    [SerializeField] SheepStates currentSheepState;
    [SerializeField] float baseSpeedMin = 15f;
    [SerializeField] float baseSpeedMax = 20f;
    [SerializeField] string jumpAnimation;
    [SerializeField] float jumpSpeed = 8f;
    OffMeshLink link;
    float oldLinkCost;
    [SerializeField] float invulnTimeOnSpawn = 1.5f;
    bool isJumping = false;
    float storedSpeed;
    float baseSpeedCurrent;
    float agentStoppingDistance;

    [Header("Follow State Variables")]
    [SerializeField] float avoidPlayerDistance;
    [SerializeField] float avoidPlayerSpeed = 40f;
    [SerializeField] float followStoppingDistanceMin = 5f;
    [SerializeField] float followStoppingDistanceMax = 10f;

    [Header("Black Sheep Variables")]
    public bool isBlackSheep = false;
    [SerializeField] GameObject blackSheepParticles;
    public Attack selfDamage;

    [Header("Pet Sheep Stuff")]
    [SerializeField] ParticleSystem petSheepParticles;
    [SerializeField] string petAnimation;

	[Header("Sounds")]
	[SerializeField] FMODUnity.EventReference biteSound;
	[SerializeField] FMODUnity.EventReference petSound;
	FMODUnity.StudioEventEmitter walker;


	[Header("Wander State Variables")]
    [SerializeField] float wanderSpeed = 10f;
    [SerializeField] float wanderRadius;
    [SerializeField] float wanderStopDistance;
    [SerializeField] float wanderDelayMin = 1f;
    [SerializeField] float wanderDelayMax = 3f;
    float currentTimeWanderStopped = 0f;
    bool canWander = true;

    [Header("Attack State Variables")]
	[SerializeField] LayerMask enemyLayer;
    [SerializeField] float attackDetectionRadius;
    [SerializeField] Damageable attackTargetCurrent;
    [SerializeField] List<Damageable> attackTargets;
    [SerializeField] string attackAnimation;
    [SerializeField] float attackCooldown = 1.5f;
    [SerializeField] float distanceToAttack;
    [SerializeField] float attackStopDistance;

    public SheepAttack attackBase;
    //public float attackDamage = 5f;
    [SerializeField] bool canAttack = true;

	[Header("Charge State Variables")]
	[SerializeField]  SheepStampedeBehavior stampede;
	public GameObject chargeParticles;

    [Header("Defend State Variables")]
	[SerializeField] SheepVortexBehavior vortex;

    [Header("Stun State Variables")]
    [SerializeField] float StunTime = 1;
    [SerializeField] float fallRate = 50;
	Coroutine hitstunCo;
    bool isGrounded;
	[HideInInspector] // hold new position so that constructs can query it even if sheep is still lerping to it
	public Vector3 constructPos;

	[Header("DEBUG")]
	public PlayerSheepAI leaderSheep;
	public int sheepPoolIndex;
	public List<PlayerSheepAI> activeSheepPool; // pointer to other sheep

	// components
    Transform player;
    [HideInInspector] public NavMeshAgent agent;
    Animator animator;
	public delegate void callSheep(int x, PlayerSheepAI y);
	public callSheep RemoveSheep;

	override protected void Start()
    {
        base.Start();

        //make sure off mesh link is null
        link = null;

        walker = GetComponent<FMODUnity.StudioEventEmitter>();

		animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        baseSpeedCurrent = GetRandomSheepBaseSpeed();

        player = WorldState.instance.player.transform;

        //get random follow stopping distance
        //this prevents sheep from clumping up and getting jittery when in a flock behind player
        agentStoppingDistance = Random.Range(followStoppingDistanceMin, followStoppingDistanceMax);

        isInvulnerable = true;
        Invoke("DisableSpawnInvuln", invulnTimeOnSpawn);

        //check black sheep stuff
        if (isBlackSheep)
			blackSheepParticles.SetActive(true);

        //if default state is wander, go wandering
        if (currentSheepState == SheepStates.WANDER)
        {
			SheepSetDestination(transform.position);
            agent.speed = wanderSpeed;
            agent.stoppingDistance = wanderStopDistance;
			SetSheepState(SheepStates.WANDER);

            GoWandering();
        }
    }

    private void OnDestroy()
    {
        ReleaseOffmeshLink();
    }

    void DisableSpawnInvuln()
    {
        isInvulnerable = false;
    }

    void UpdateAnimation()
    {
        if (agent.velocity.sqrMagnitude > 1) 
			animator.SetBool("isMoving", true);
        else 
			animator.SetBool("isMoving", false);

        //jump
        if (agent.isOnOffMeshLink && !isJumping )
        {
            AcquireOffmeshLink();
            storedSpeed = agent.speed;
            agent.speed = jumpSpeed;
            isJumping = true;
            animator.Play(jumpAnimation);
        }
        if (isJumping && !agent.isOnOffMeshLink)
        {
            ReleaseOffmeshLink();
            agent.speed = storedSpeed;
            isJumping = false;
        }

		if (!walker.IsPlaying() && agent.enabled && agent.velocity.magnitude > 0.5f)
			walker.Play();
		else if (!agent.enabled || agent.velocity.magnitude <= 0.5f)
			walker.Stop();
		walker.SetParameter("Speed", agent.velocity.magnitude);
	}

    void AcquireOffmeshLink()
    {
        if (link == null)
        {
            link = agent.currentOffMeshLinkData.offMeshLink;
            oldLinkCost = link.costOverride;
            link.costOverride = 1000.0f;
        }
    }

    void ReleaseOffmeshLink()
    {
        if (link != null)
        {
            link.costOverride = oldLinkCost;
            link = null;
        }
    }

    public void GothMode()
    {
        isBlackSheep = true;
        blackSheepParticles.SetActive(true);
    }

    private void Update()
    {
        UpdateAnimation();

        //state machine
        switch (currentSheepState)
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
            case SheepStates.STAMPEDE:
                {
					stampede.AbilityUpdate(this);
                    break;
                }
            case SheepStates.VORTEX:
                {
					vortex.AbilityUpdate(this);
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

	public void DealDamage(Collider target, SheepAttack theAttack, bool blackSheepDamage)
	{
		FMODUnity.RuntimeManager.PlayOneShotAttached(biteSound, gameObject);

		if (blackSheepDamage)
		{
			//subtract 1 from health
			TakeDamage(selfDamage, transform.forward);

			Instantiate(theAttack.explosionEffect, transform.position, transform.rotation);
			target?.GetComponent<EnemyAI>().TakeDamage(theAttack, transform.forward);
		}
		else
			target?.GetComponent<EnemyAI>().TakeDamage((Attack)theAttack, transform.forward);
	}
    private void OnTriggerEnter(Collider other)
    {
        switch (currentSheepState)
        {
            case SheepStates.STAMPEDE:
                {
					stampede.AbilityTriggerEnter(this, other);
                    break;
                }
            case SheepStates.VORTEX:
                {
					vortex.AbilityTriggerEnter(this, other);
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
    //this is called to kill an indvidual sheep and remove it from list
    public void KillSheep()
    {
		walker.Stop();

		RemoveSheep(sheepType, this);
		Destroy(gameObject);
    }

	void SheepSetDestination(Vector3 dest)
	{
		if (!agent.isOnNavMesh && !agent.isOnOffMeshLink)
		{
			print(gameObject + " failed to find a destination "+isGrounded);
			GibSheep();
			KillSheep();
		}
		else
			agent.SetDestination(dest);
	}

    public void GibSheep()
    {
        Instantiate(gibs, transform.position, transform.rotation);
    }

    public void PetSheep()
    {
        petSheepParticles.Play();
		FMODUnity.RuntimeManager.PlayOneShotAttached(petSound, gameObject);

		animator.Play(petAnimation);
    }

    public void RecallSheep()
    {
        // sheep cant be recalled when stunned OR DEFENDING
        if (currentSheepState == SheepStates.STUN || currentSheepState == SheepStates.VORTEX)
            return;

        // if the sheep is too high up, stun it first so that it gets closer to the ground
        if (!Physics.Raycast(transform.position, Vector3.down, 10, LayerMask.GetMask("Ground")))
        {
			SetHitstun(SheepStates.FOLLOW_PLAYER);
            return;
        }

		animator.SetBool("isDefending", false);

		SetSheepState(SheepStates.FOLLOW_PLAYER);

        EndConstruct();
    }
    public SheepStates GetSheepState()
    {
        return currentSheepState;
    }
	public void SetSheepState(SheepStates newstate)
	{
		if (hitstunCo != null)
			StopCoroutine(hitstunCo);
		currentSheepState = newstate;

		// reset speed (notable for stampede)
		if (currentSheepState == SheepStates.WANDER)
		{
			agent.speed = wanderSpeed;
			agent.stoppingDistance = wanderStopDistance;
			agent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
		}
	}
    public bool IsCommandable()
    {
        return currentSheepState == SheepStates.STAMPEDE || currentSheepState == SheepStates.FOLLOW_PLAYER || currentSheepState == SheepStates.WANDER;
    }
    float GetRandomSheepBaseSpeed()
    {
        float speed = Random.Range(baseSpeedMin, baseSpeedMax);
        return speed;
    }

	public Animator GetAnimator()
	{
		return animator;
	}
    #endregion

    #region Health
    protected override void OnDeath()
    {
		FMODUnity.RuntimeManager.PlayOneShot(deathSound, transform.position);

		Instantiate(gibs, transform.position, transform.rotation);
        KillSheep();
    }
    public override void TakeDamage(Attack atk, Vector3 attackForward)
    {
		if (atk.DealsHitstun)
			SetHitstun(SheepStates.WANDER);

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
			SheepSetDestination(avoidDestination);
            return;
        }
        else if (!isJumping)
        {
            //set speed and follow distance
            agent.speed = baseSpeedCurrent;
            agent.stoppingDistance = agentStoppingDistance;
			SheepSetDestination(player.position);
        }

    }
    #endregion

    #region Sheep Stun 
	public void SetHitstun(SheepStates stateAfterStun)
	{
		if (this == null || currentSheepState == SheepStates.VORTEX)
			return;
		if (hitstunCo != null)
			StopCoroutine(hitstunCo);
		hitstunCo = StartCoroutine(OnHitStun(SheepStates.WANDER));
	}
	IEnumerator OnHitStun(SheepStates stateAfterStun)
    {
		// save current state and set to Hitstun
		SheepStates origState = currentSheepState;
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

		// if sheep were attacking, they can resume attacking 
		// the condition is needed because actions like vortex and construct should not be resumed
		currentSheepState = (origState == SheepStates.ATTACK) ? origState : SheepStates.WANDER;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (currentSheepState == SheepStates.STUN && collision.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
			//Debug.Log("sheep collided with " + collision.gameObject);
            isGrounded = true;

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
        else agent.destination = transform.position;

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

        //go wandering in case you get stuck
        if(agent.velocity.magnitude <= 0.25f && canWander)
        {
            currentTimeWanderStopped += Time.deltaTime;
            if (currentTimeWanderStopped >= wanderDelayMax + 0.1f) GoWandering();
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
		if (attackTargetCurrent != null)
			SheepSetDestination(attackTargetCurrent.transform.position);


        if (canAttack)
        {
            //first check if we have a target and are in range
            if (attackTargetCurrent != null && Vector3.Distance(transform.position, attackTargetCurrent.transform.position) <= distanceToAttack)
            {
                //if the target is executable, remove them from the list
                if (attackTargetCurrent is EnemyAI && ((EnemyAI)attackTargetCurrent).GetState() == EnemyStates.EXECUTABLE)
                {
                    attackTargets.Remove(attackTargetCurrent);
                    attackTargetCurrent = null;
                }

                else
                {
					SheepSetDestination(transform.position);
                    transform.LookAt(attackTargetCurrent.transform);
                    animator.Play(attackAnimation);
                    canAttack = false;
                    StartCoroutine(AttackCooldown());
                }
            }
            //if no target, go to next in list!
            else if (attackTargetCurrent == null)
            {
                attackTargets.Remove(attackTargetCurrent);
                attackTargetCurrent = GetAttackTarget();

                //still no target? then go back to wander state
                if (attackTargetCurrent == null)
                {
                    Debug.Log("Attack target is null, going to wander");
					SheepSetDestination(transform.position);
                    agent.speed = wanderSpeed;
                    agent.stoppingDistance = wanderStopDistance;
					SetSheepState(SheepStates.WANDER);

                    GoWandering();
                }
            }
        }

    }
	Damageable GetAttackTarget()
    {
        //find targets to attack
        //FindAttackTargets();

        if(attackTargets.Count > 0)
        {
            //return a random one
            int rand = Random.Range(0, attackTargets.Count);
            return attackTargets[rand];
        }
        else return null;
    }

    public void CreateListOfAttackTargets(Vector3 targetPos, float targetRadius)
    {
        //clear old list
        attackTargets.Clear();
        attackTargetCurrent = null;

        //check if there are enemies nearby specified area
        //Demetri I am using Physics.OverlapSphere against your wishes
        Collider[] enemyHits = (Physics.OverlapSphere(targetPos, targetRadius, enemyLayer));
        foreach (Collider enemy in enemyHits)
        {
			Damageable enemyDamageable = enemy.GetComponent<Damageable>();
			if (enemyDamageable != null && (!(enemyDamageable is EnemyAI) || ((EnemyAI)enemyDamageable).GetState() != EnemyStates.EXECUTABLE))
				attackTargets?.Add(enemyDamageable);
        }

		//start attacking!
		SetSheepState(SheepStates.ATTACK);
        agent.stoppingDistance = attackStopDistance;
        agent.speed = baseSpeedCurrent;
    }

    //depreciated function, here as a reference now
    void FindAttackTargets()
    {
        attackTargets.Clear();

        //check if there are enemies nearby
        //Demetri I am using Physics.OverlapSphere against your wishes
        Collider[] enemyHits = (Physics.OverlapSphere(transform.position, attackDetectionRadius, enemyLayer));
        foreach(Collider enemy in enemyHits)
        {
			Damageable enemyDamageable = enemy.GetComponent<Damageable>();
			if (enemyDamageable != null && (!(enemyDamageable is EnemyAI) || ((EnemyAI)enemyDamageable).GetState() != EnemyStates.EXECUTABLE))
				attackTargets?.Add(enemyDamageable);
        }
    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    #endregion

    public void BeginCharge(Vector3 theChargePosition)
    {
		stampede.Begin(this, theChargePosition);
    }

    #region Sheep Construct / Lift
	public void StartLift()
	{
		SetSheepState(SheepStates.LIFT);
		agent.enabled = false;
		rb.isKinematic = true;
	}
	public void CancelLift()
	{
		SetSheepState(SheepStates.FOLLOW_PLAYER);
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
			SetHitstun(SheepStates.FOLLOW_PLAYER);
	}

	public void DoConstruct(Vector3 newPos)
	{
		constructPos = newPos;
		SetSheepState(SheepStates.CONSTRUCT);
		agent.enabled = false;

		rb.constraints = RigidbodyConstraints.FreezeAll;

		rb.angularVelocity = Vector3.zero;
		rb.velocity = Vector3.zero;
	}
	public void EndConstruct(bool callRemove = true)
	{
		agent.enabled = true;
		//gameObject.layer = SheepLayer;     

		// if not already changed, make sure its not on CONSTRUCT
		if (currentSheepState == SheepStates.CONSTRUCT)
			SetHitstun(SheepStates.WANDER);
	}
	#endregion
}