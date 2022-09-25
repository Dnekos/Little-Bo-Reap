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
	CONSTRUCT = 4
}

[RequireComponent(typeof(NavMeshAgent))]
public class PlayerSheepAI : MonoBehaviour
{
    [Header("Sheep State Variables")]
    [SerializeField] SheepStates currentSheepState;
    [SerializeField] float baseSpeedMin = 15f;
    [SerializeField] float baseSpeedMax = 20f;
    float baseSpeedCurrent;
    float agentStoppingDistance;
    Transform player;
    NavMeshAgent agent;

    [Header("Follow State Variables")]
    [SerializeField] float avoidPlayerDistance;
    [SerializeField] float avoidPlayerSpeed = 40f;

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

	//[Header("Construct State Variables")]
	SheepHolder owningConstruct;
   

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        baseSpeedCurrent = GetRandomSheepBaseSpeed(); ;
        agentStoppingDistance = agent.stoppingDistance;
        player = GameManager.Instance.GetPlayer();
    }

    private void Update()
    {
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
    public void KillSheep()
    {
        Destroy(gameObject);
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
	//NEED TO EDIT
	//RIGHT NOW THIS SHIT IS CALLED EERY FRAME FOR EVERY SHEEP
	//THIS IS BAD I SHOULD MAKE FIX IT MAYBE D:
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
			currentSheepState = SheepStates.WANDER;
	}
	#endregion
}
