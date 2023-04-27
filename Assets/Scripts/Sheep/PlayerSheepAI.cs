using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum SheepStates
{
    FOLLOW_PLAYER = 0,
    WANDER = 1,
    ABILITY = 2,
	CONSTRUCT = 4,
    ATTACK = 5,
	STUN = 6,
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

	[Header("Jumping")]
    [SerializeField] string jumpAnimation;
    [SerializeField] string jumpLandAnimation;
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
    [SerializeField] float maxSize;
    [SerializeField] float minSize;
    [SerializeField] GameObject meshParent;
    public float currentSize;

    [Header("Easter Eggs!")]
    [SerializeField] List<GameObject> easterEggMeshes;
    [SerializeField] float percentChanceToEasterEgg = 1f;

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

	[Header("Active Ability")]
	public SheepBehavior ability;
	public GameObject chargeParticles;

    [Header("Stun State Variables")]
    [SerializeField] float StunTime = 1;
    [SerializeField] float fallRate = 50;
	Coroutine hitstunCo;
    bool isGrounded;

	// Construct values
	[HideInInspector] // hold new position so that constructs can query it even if sheep is still lerping to it
	public Vector3 constructPos;
	[HideInInspector]
	public float Radius;
	public const int DamageFromConstruct = 0;

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

        float size = Random.Range(minSize, maxSize);
        meshParent.transform.localScale = new Vector3(size, size, size);
        currentSize = size;

        if (sheepType == 2)
        {
            MaxHealth += WorldState.instance.passiveValues.fluffyHealth;
        }

		walker = GetComponent<FMODUnity.StudioEventEmitter>();

		animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        baseSpeedCurrent = GetRandomSheepBaseSpeed();

        player = WorldState.instance.player.transform;

		Radius = GetRadius();

		Initialize();

		constructPos = Vector3.negativeInfinity;
	}

	/// <summary>
	/// everything needed when the sheep is reenabled from the pool
	/// </summary>
	public void Initialize()
	{
		// prevent them from getting stuck in the summon animation might be unneeded
		animator.Rebind();
		animator.Play("Test_Sheep_Summon", -1, 0);

		agent.enabled = true;

		Health = MaxHealth;

		//make sure off mesh link is null
		link = null;

        Debug.Log("init sheep");
        //clear attack data
        canAttack = true;
        attackTargetCurrent = null;
        attackTargets.Clear();

        //if (System.DateTime.Today.Month == 4 && System.DateTime.Today.Day == 20) Debug.Log("April foolks!");

        //hehe easter eggs
        //turn em all off
        for(int i = 0; i < easterEggMeshes.Count; i++)
        {
            easterEggMeshes[i].SetActive(false);
        }
        //get random number
        float easterEgg = Random.Range(0f, 100f);
        if(easterEgg <= percentChanceToEasterEgg || System.DateTime.Today.Month == 4 && System.DateTime.Today.Day == 1 || System.DateTime.Today.Month == 10 && System.DateTime.Today.Day == 31)
        {
            int easterEggIndex = Random.Range(0, easterEggMeshes.Count);
            easterEggMeshes[easterEggIndex].SetActive(true);
        }



        //get random follow stopping distance
        //this prevents sheep from clumping up and getting jittery when in a flock behind player
        agentStoppingDistance = Random.Range(followStoppingDistanceMin, followStoppingDistanceMax);

		isInvulnerable = true;
		Invoke("DisableSpawnInvuln", invulnTimeOnSpawn);

		if (hitstunCo != null)
			StopCoroutine(hitstunCo);

		//remove all velocity
		rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

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

        //stun
        if (currentSheepState == SheepStates.STUN)
            animator.SetBool("isStunned", true);
        else
            animator.SetBool("isStunned", false);

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
            animator.Play(jumpLandAnimation);
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
            case SheepStates.ABILITY:
                {
					ability.AbilityUpdate(this);
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
		// make sure it has health to be damaged
		Damageable targetHealth = target.GetComponent<Damageable>();
		if (targetHealth != null)
		{
			FMODUnity.RuntimeManager.PlayOneShotAttached(biteSound, gameObject);

			// set attack
			Attack activeAtk = theAttack;
			// if black sheep, use that attack instead
			if (blackSheepDamage)
			{
				activeAtk = theAttack.BSAttack;

				// Take self damage
				TakeDamage(selfDamage, transform.forward);
			}

			// damage effect
			if (blackSheepDamage || sheepType == 1)
				Instantiate(theAttack.explosionEffect, transform.position, transform.rotation);

			if (sheepType == 1) //if ram, use ram damage/knockback variables
				targetHealth.TakeDamage(activeAtk, transform.forward,
					WorldState.instance.passiveValues.ramDamage, WorldState.instance.passiveValues.ramKnockback);
			else
				targetHealth.TakeDamage(activeAtk, transform.forward);
		}
	}
        
		
	
    private void OnTriggerEnter(Collider other)
    {
        switch (currentSheepState)
        {
            case SheepStates.ABILITY:
                {
					ability.AbilityTriggerEnter(this, other);
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

        //defualt to not black sheep
        isBlackSheep = false;
        blackSheepParticles.SetActive(false);

        RemoveSheep(sheepType, this);
		gameObject.SetActive(false);
		//CancelLift();

		ReleaseOffmeshLink();

		//Destroy(gameObject);
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
        //Instantiate(gibs, transform.position, transform.rotation);
		WorldState.instance.pools.DequeuePooledObject(gibs, transform.position + Vector3.up * 1.4f, Quaternion.identity);
	}
	public bool IsCommandable()
	{
		return (currentSheepState == SheepStates.ABILITY && ability.IsRecallable(this)) || currentSheepState == SheepStates.FOLLOW_PLAYER || currentSheepState == SheepStates.WANDER;
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
	public void SetStopDist(float stopDist)
	{
		agent.stoppingDistance = stopDist;
	}
	#endregion

	#region SheepActions
	public void PetSheep()
    {
        petSheepParticles.Play();
		FMODUnity.RuntimeManager.PlayOneShotAttached(petSound, gameObject);

		animator.Play(petAnimation);
    }

    public void RecallSheep()
    {
        // sheep cant be recalled when stunned OR DEFENDING
        if (currentSheepState == SheepStates.STUN || (currentSheepState == SheepStates.ABILITY && !ability.IsRecallable(this)))
            return;

        // if the sheep is too high up, stun it first so that it gets closer to the ground
        if (!Physics.Raycast(transform.position, Vector3.down, 10, LayerMask.GetMask("Ground")))
        {
			SetHitstun(SheepStates.FOLLOW_PLAYER);
            return;
        }

		agent.enabled = true;
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
		{
			StopCoroutine(hitstunCo);
			rb.isKinematic = true;
		}

		// if they were in an ability, end it, this is messy and gross
		if (currentSheepState == SheepStates.ABILITY)
		{
			currentSheepState = newstate;
			ability.End(this);
		}

		currentSheepState = newstate;


		// reset speed (notable for stampede)
		switch (currentSheepState)
		{
			case SheepStates.ATTACK:
				agent.speed = baseSpeedCurrent;
				agent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
				break;
			case SheepStates.WANDER:
				agent.speed = wanderSpeed;
				agent.stoppingDistance = wanderStopDistance;
				agent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
				break;
			case SheepStates.FOLLOW_PLAYER:
				agent.stoppingDistance = attackStopDistance;
				agent.speed = baseSpeedCurrent;
				agent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
				break;
		}
	}

    #endregion

    #region Health
    protected override void OnDeath()
    {
		FMODUnity.RuntimeManager.PlayOneShot(deathSound, transform.position);

		WorldState.instance.pools.DequeuePooledObject(gibs, transform.position, transform.rotation);
        KillSheep();
    }
    public override void TakeDamage(Attack atk, Vector3 attackForward, float damageAmp = 1, float knockbackMultiplier = 1)
    {
        if (atk.DealsHitstun && currentSheepState != SheepStates.CONSTRUCT)
            SetHitstun(SheepStates.WANDER);

		Debug.Log("Took Damage in a " + currentSheepState.ToString() + " State.");

		switch (currentSheepState)
        {
            case SheepStates.LIFT:  //If you are in a LIFT state, take damage modified by the ConstructDR multiplier
                base.TakeDamage(atk, attackForward, WorldState.instance.passiveValues.builderConstructDR);
                break;
            case SheepStates.CONSTRUCT:  //If you are in a CONSTRUCT state, take damage modified by the ConstructDR multiplier
				if (knockbackMultiplier == DamageFromConstruct) // if theres 0 knockback, we can assume its from the construct. damageAmp is set to be a % of the damage, spread between sheep
				{
					float damageMult = (WorldState.instance.passiveValues.builderConstructDR <= 0) ? damageAmp : WorldState.instance.passiveValues.builderConstructDR * damageAmp;
					base.TakeDamage(atk, attackForward, damageMult, DamageFromConstruct);
				}
                break;
            case SheepStates.ABILITY:  //If you are in a STAMPEDE state, take damage modified by the StampedeDR multiplier
				if (ability is SheepStampedeBehavior)
					base.TakeDamage(atk, attackForward, WorldState.instance.passiveValues.ramChargeDR);
				else
				{
					if (sheepType != 2) //If this is a fluffy sheep, apply the fluffy knockback resistance multiplier
					{
						base.TakeDamage(atk, attackForward);
					}
					else
					{
						Debug.Log("Fluffy Took Damage");
						base.TakeDamage(atk, attackForward, 0.0f, WorldState.instance.passiveValues.fluffyKnockResist);
					}
				}
				break;

            default: //Otherwise, take damage as normal.
                if (sheepType != 2) //If this is a fluffy sheep, apply the fluffy knockback resistance multiplier
                {
                    base.TakeDamage(atk, attackForward);
                }
                else
                {
                    Debug.Log("Fluffy Took Damage");
                    base.TakeDamage(atk, attackForward, 0.0f, WorldState.instance.passiveValues.fluffyKnockResist);
                }
                break;
        }
    }
	protected override void PlayHurtSound()
	{
		if (currentSheepState != SheepStates.CONSTRUCT)
		{
			// muted baa for stampede
			if (sheepType == 1 && WorldState.instance.PersistentData.activeUpgrades.HasFlag(SaveData.Upgrades.RamChargeDR))
			{
				FMOD.Studio.EventInstance instance = FMODUnity.RuntimeManager.CreateInstance(hurtSound);
				instance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));
				instance.setParameterByName("Progression", 1); // the line that makes it muted, the rest is copied from the internal FMOD PlayOneshot
				instance.start();
				instance.release();
			}
			else
				base.PlayHurtSound();
		}
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
		if (this == null || !gameObject.activeInHierarchy || currentSheepState == SheepStates.ABILITY)
			return;

		// coroutine
		if (hitstunCo != null)
			StopCoroutine(hitstunCo);
		hitstunCo = StartCoroutine(OnHitStun(stateAfterStun));

		// get out of any constructs
		EndConstruct();
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
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        yield return new WaitForSeconds(StunTime);

        // wait until grounded
        yield return new WaitUntil(() => isGrounded);
        //yield return new WaitForSeconds(0.1f);

       // rb.isKinematic = true;
		agent.enabled = true;
		if (!agent.isOnNavMesh)
		{
			SetHitstun(stateAfterStun);
			yield break;
		}

		//rb.isKinematic = true;
		rb.constraints = RigidbodyConstraints.FreezeAll;

		rb.angularVelocity = Vector3.zero;
		rb.velocity = Vector3.zero;
		//Debug.Log(gameObject.name + " ending stunn");
		// if sheep were attacking, they can resume attacking 
		// the condition is needed because actions like vortex and construct should not be resumed
		SetSheepState(stateAfterStun);//(origState == SheepStates.ATTACK) ? origState : SheepStates.WANDER);

	}

    private void OnCollisionStay(Collision collision)
    {
        if (currentSheepState == SheepStates.STUN && collision.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
			//Debug.Log("sheep collided with " + collision.gameObject);
            isGrounded = true;
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
		if (agent.isOnNavMesh)
		{
			if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, wanderRadius, 1))
			{
				//get charge
				destination = hit.position;

				//set agent destination
				agent.destination = destination;
			}
			else
				agent.destination = transform.position;
		}
		else if (!agent.isOnOffMeshLink)
			SetHitstun(SheepStates.WANDER);

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


        if (canAttack && IsCommandable())
        {
            //first check if we have a target and are in range
            if (attackTargetCurrent != null && Vector3.Distance(transform.position, attackTargetCurrent.transform.position) - attackTargetCurrent.GetRadius() <= distanceToAttack)
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
			Damageable enemyDamageable = enemy.GetComponent<EnemyBase>();
			if (enemyDamageable != null && (!(enemyDamageable is EnemyAI) || ((EnemyAI)enemyDamageable).GetState() != EnemyStates.EXECUTABLE))
				attackTargets?.Add(enemyDamageable);
        }

        //set agent stopping speed here for now, where did it go? idk someone removed it so here it is
        agent.stoppingDistance = attackStopDistance;

        //start attacking!
        SetSheepState(SheepStates.ATTACK);
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

	#region Active Ability
	// TODO: these two needing specific inputs is the only hangup
	public void BeginAbility(Vector3 theChargePosition)
    {
		ability.Begin(this, theChargePosition);
    }
	public void EndAbility(GameObject Projectile)
	{
		ability.End(this, Projectile);
	}
	#endregion

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
	public void EndConstruct()
	{
		//agent.enabled = true;
		//gameObject.layer = SheepLayer;     

		// if not already changed, make sure its not on CONSTRUCT
		if (currentSheepState == SheepStates.CONSTRUCT)
			SetHitstun(SheepStates.FOLLOW_PLAYER);
		if (ability is SheepVortexBehavior)
			ability.End(this);
	}
	#endregion
}