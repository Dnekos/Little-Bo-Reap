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
	STUN // TODO, make this the same as the enemy's
}

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerSheepAI : MonoBehaviour
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
	Rigidbody rb;

   

	[Header("Follow State Variables")]
    [SerializeField] float avoidPlayerDistance;
    [SerializeField] float avoidPlayerSpeed = 40f;

    [Header("Leader Variables")]
    public PlayerSheepAI leaderSheep;
    public bool isLeader;

    [Header("Wander State Variables")]
    [SerializeField] float wanderSpeed = 10f;
    [SerializeField] float wanderRadius;
    [SerializeField] float wanderStopDistance;
    [SerializeField] float wanderDelayMin = 1f;
    [SerializeField] float wanderDelayMax = 3f;
    bool canWander = true;

    [Header("Charge State Variables")]
    [SerializeField] float chargeSpeed = 35f;
    [SerializeField] float chargePointRadius = 10f;
    [SerializeField] float chargeStopDistance = 0f;
    [SerializeField] float chargeEndDistance = 1f;
    Vector3 chargePoint;
    bool isCharging;

    [Header("Defend State Variables")]
    [SerializeField] float defendSpeed = 35f;
    [SerializeField] float defendStopDistance = 0f;
    Transform defendPoint;

	[Header("Stun State Variables")]
	[SerializeField] float StunTime = 1;
	SheepHolder owningConstruct;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
		rb = GetComponent<Rigidbody>();
		baseSpeedCurrent = GetRandomSheepBaseSpeed(); ;
        agentStoppingDistance = agent.stoppingDistance;
        player = GameManager.Instance.GetPlayer();

      
        FindLeader();
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
        }
    }
    void CheckLeader()
    {
        if (leaderSheep == null) FindLeader();
    }

    private void Update()
    {
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
                    break;
                }
            case SheepStates.DEFEND_PLAYER:
                {
                    DoDefendPlayer();
                    break;
                }
			case SheepStates.CONSTRUCT:
				break;
            default:
                {
                    Debug.LogWarning("PlayerSheepAI should never default!");
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

		//turn on rb and turn off navmesh (turned on in GroundCheck (which cant be called when hitstunned))
		rb.isKinematic = false;
		agent.enabled = false;

		yield return new WaitForSeconds(StunTime);

		currentSheepState = stateAfterStun;
	}
	#endregion 

	#region Wander


	void DoWander()
    {
        //if stopped, pick new point to wander!
        if (Vector3.Distance(transform.position, agent.destination) <= 1f && canWander)
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
    }
    IEnumerator WanderCooldown()
    {
        float randTime = Random.Range(wanderDelayMin, wanderDelayMax);
        yield return new WaitForSeconds(randTime);
        canWander = true;
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
    public void BeginCharge(Vector3 theChargePosition)
    {
        //CHARGE!
        isCharging = true;

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

	#region Sheep Construct
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
