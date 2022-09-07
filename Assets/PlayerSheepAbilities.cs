using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;

public class PlayerSheepAbilities : MonoBehaviour
{
    [Header("Base Sheep Variables")]
    [SerializeField] List<PlayerSheepAI> activeSheep;
    [SerializeField] GameObject sheepPrefab;

    [Header("Sheep Summon Variables")]
    [SerializeField] float amountToSummon = 3f;
    [SerializeField] float summonRadius = 20f;
    [SerializeField] float summonIntervalMin = 0f;
    [SerializeField] float summonIntervalMax = 0.5f;
    [SerializeField] float summonCooldown = 5f;
    [SerializeField] string summonAnimation;
    bool canSummonSheep = true;

    [Header("Sheep Charge Variables")]
    [SerializeField] GameObject sheepChargePointPrefab;
    [SerializeField] LayerMask chargeTargetLayers;
    [SerializeField] string chargeAnimation;
    GameObject sheepChargePoint;
    Vector3 chargePosition;
    bool isPreparingCharge;

    Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    private void Update()
    {
        CheckCharge();
    }

    #region Sheep Summon and Recall
    public void OnRecallSheep(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            //play animation
            animator.Play(summonAnimation);

            //recall all sheep!
            for (int i = 0; i < activeSheep.Count; i++)
            {
                activeSheep[i]?.RecallSheep();
            }
        }
    }
    public void OnSummonSheep(InputAction.CallbackContext context)
    {
        if(context.performed && canSummonSheep)
        {
            //disallow summoning
            canSummonSheep = false;

            //play animation
            animator.Play(summonAnimation);

            //delete all active sheep
            for(int i = 0; i < activeSheep.Count; i++)
            {
                activeSheep[i].KillSheep();
            }
            activeSheep.Clear();

            //summon sheep!
            for(int i = 0; i < amountToSummon; i++)
            {
                StartCoroutine(SummonSheep());
            }

            //start cooldown
            StartCoroutine(SummonSheepCooldown());
        }
    }
    IEnumerator SummonSheep()
    {
        //get random interval
        float summonDelay = Random.Range(summonIntervalMin, summonIntervalMax);

        yield return new WaitForSeconds(summonDelay);

        //get random point inside summoning radius
        Vector3 summonPosition = Vector3.zero;
        Vector3 randomPosition = Random.insideUnitSphere * summonRadius;
        randomPosition += transform.position;

        //if inside navmesh, spawn sheep!
        if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, summonRadius, 1))
        {
            //get spawn position
            summonPosition = hit.position;

            //spawn sheep
            var sheep = Instantiate(sheepPrefab, summonPosition, Quaternion.identity) as GameObject;
            activeSheep.Add(sheep.GetComponent<PlayerSheepAI>());
        }
        else
        {
            Debug.Log("Sheep could not be summoned! Are you too far from a navmesh?");
        }
    }
    IEnumerator SummonSheepCooldown()
    {
        yield return new WaitForSeconds(summonCooldown);
        canSummonSheep = true;
    }
    #endregion

    #region Sheep Charge
    public void OnSheepCharge(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            //spawn icon
            var chargePoint = Instantiate(sheepChargePointPrefab, transform.position, Quaternion.identity) as GameObject;
            sheepChargePoint = chargePoint;

            //prepare to charge
            isPreparingCharge = true;
        }

        if(context.canceled)
        {
            //stop charging
            isPreparingCharge = false;

            //play animation
            animator.Play(chargeAnimation);

            //get rid of icon
            Destroy(sheepChargePoint);

            //send sheep to point if valid!
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, Mathf.Infinity, chargeTargetLayers))
            {
                for(int i = 0; i <activeSheep.Count; i++)
                {
                    if(activeSheep[i].GetSheepState() != SheepStates.WANDER) activeSheep[i]?.BeginCharge(hit.point);
                }
            }
        }
    }
    void CheckCharge()
    {
        if(isPreparingCharge)
        {
            //draw ray from camera forward to point
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, Mathf.Infinity, chargeTargetLayers))
            {
                //draw charge point
                sheepChargePoint.transform.position = hit.point;
            }
            else
            {
                //draw it way the fuck down so it isnt seen
                sheepChargePoint.transform.position = new Vector3(0f, -1000f, 0f);
            }

        }
    }
    #endregion

}
