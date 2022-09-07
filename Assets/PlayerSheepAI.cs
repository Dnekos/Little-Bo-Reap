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
    Vector3 chargePoint;
    bool isCharging;

   

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = GetRandomSheepBaseSpeed(); ;
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
            default:
                {
                    Debug.LogWarning("PlayerSheepAI should never default!");
                    break;
                }
        }
    }

    public void KillSheep()
    {
        Destroy(gameObject);
    }
    public void RecallSheep()
    {
        currentSheepState = SheepStates.FOLLOW_PLAYER;
    }
    public SheepStates GetSheepState()
    {
        return currentSheepState;
    }
    float GetRandomSheepBaseSpeed()
    {
        float speed = Random.Range(baseSpeedMin, baseSpeedMax);
        return speed;
    }

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
            //otherwise follow
            agent.speed = GetRandomSheepBaseSpeed();
            agent.stoppingDistance = agentStoppingDistance;
            agent.SetDestination(player.position);
        }
    }
    
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

    void DoCharge()
    {
        //if agent reaches charge point, go into wander mode
        if(Vector3.Distance(transform.position, agent.destination) <= 1f)
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

    void DoDefendPlayer()
    {

    }

}
