using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

enum SheepStates
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
    float agentStoppingDistance;
    Transform player;
    NavMeshAgent agent;

    [Header("Follow State Variables")]
    [SerializeField] float avoidPlayerDistance;

    [Header("Wander Stae Variables")]
    [SerializeField] float wanderRadius;
   

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
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
            agent.stoppingDistance = 0;
            agent.SetDestination(avoidDestination);
        }
        else
        {
            //otherwise follow
            agent.stoppingDistance = agentStoppingDistance;
            agent.SetDestination(player.position);
        }
    }
    
    void DoWander()
    {

    }

    void DoCharge()
    {

    }

    void DoDefendPlayer()
    {

    }

}
