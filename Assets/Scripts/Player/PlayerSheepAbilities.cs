using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;

public enum SheepTypes
{
    BUILD = 0,
    RAM = 1,
    FLUFFY = 2
}
public class PlayerSheepAbilities : MonoBehaviour
{
    [Header("Sheep Flock Variables")]
    [SerializeField] List<PlayerSheepAI> activeSheepBuild;
    [SerializeField] List<PlayerSheepAI> activeSheepRam;
    [SerializeField] List<PlayerSheepAI> activeSheepFluffy;
    [SerializeField] GameObject sheepPrefabBuild;
    [SerializeField] GameObject sheepPrefabRam;
    [SerializeField] GameObject sheepPrefabFluffy;
    [SerializeField] SheepTypes currentFlockType;
    int currentFlockIndex;

    [Header("Sheep Summon Variables")]
    [SerializeField] int amountToSummonBuild;
    [SerializeField] int amountToSummonRam;
    [SerializeField] int amountToSummonFluffy;
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

    [Header("Sheep Defend Variables")]
    [SerializeField] List<Transform> defendPoints;
    [SerializeField] Transform defendPointPivot;
    [SerializeField] float defendPivotRotateSpeed = 1f;
    [SerializeField] float defendDistance = 3f;
    [SerializeField] string defendAnimation;

    [Header("Sheep Launch Variables")]
    [SerializeField] float launchForce = 2500f;
    [SerializeField] float launchForceLift = 250f;
    [SerializeField] GameObject launchProjectile;
    [SerializeField] Transform launchOrigin;
    [SerializeField] float launchCooldown = 1f;
    [SerializeField] float minDistanceToLaunch = 10f;
    [SerializeField] string launchAnimation;
    bool canLaunch = true;

    Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();

        currentFlockIndex = (int)currentFlockType;

        //set defend distance of each defence point
        for(int i = 0; i < defendPoints.Count; i++)
        {
            defendPoints[i].position += defendPoints[i].forward * defendDistance;
        }
    }
    private void Update()
    {
        CheckCharge();
        CheckDefend();
    }

    #region Sheep Flock Functions
    public void OnChangeSheepFlock(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            //go to next sheep type
            currentFlockIndex++;
            if (currentFlockIndex >= 3) currentFlockIndex = 0; //dont try to set to an enum that doesnt exist 
            currentFlockType = (SheepTypes)currentFlockIndex;

            Debug.Log("Current Flock is: " + currentFlockType);
        }
    }
    public List<PlayerSheepAI> GetCurrentSheepFlock(SheepTypes theFlockType)
    {
        switch(theFlockType)
        {
            case SheepTypes.BUILD:
                {
                    return activeSheepBuild;
                }
            case SheepTypes.RAM:
                {
                    return activeSheepRam;
                }
            case SheepTypes.FLUFFY:
                {
                    return activeSheepFluffy;
                }
            default:
                {
                    Debug.LogWarning("GetCurrentSheepFlock defaulted! This should never happen!!");
                    return activeSheepBuild;
                }
        }
    }
    public GameObject GetCurrentSheepPrefab(SheepTypes theFlockType)
    {
        switch (theFlockType)
        {
            case SheepTypes.BUILD:
                {
                    return sheepPrefabBuild;
                }
            case SheepTypes.RAM:
                {
                    return sheepPrefabRam;
                }
            case SheepTypes.FLUFFY:
                {
                    return sheepPrefabFluffy;
                }
            default:
                {
                    Debug.LogWarning("GetCurrentSheepPrefab defaulted! This should never happen!!");
                    return sheepPrefabBuild;
                }
        }
    }
    public int GetSheepAmountToSummon(SheepTypes theFlockType)
    {
        switch (theFlockType)
        {
            case SheepTypes.BUILD:
                {
                    return amountToSummonBuild;
                }
            case SheepTypes.RAM:
                {
                    return amountToSummonRam;
                }
            case SheepTypes.FLUFFY:
                {
                    return amountToSummonFluffy;
                }
            default:
                {
                    Debug.LogWarning("GetSheepAmountToSummon defaulted! This should never happen!!");
                    return amountToSummonBuild;
                }
        }
    }
    #endregion

    #region Sheep Summon and Recall
    public void OnRecallSheep(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            //get flock type
            SheepTypes flockType = currentFlockType;

            //play animation
            animator.Play(summonAnimation);

            //recall current flock!
            for (int i = 0; i < GetCurrentSheepFlock(flockType).Count; i++)
            {
                GetCurrentSheepFlock(flockType)[i]?.RecallSheep();
            }
        }
    }
    public void OnRecallAllSheep(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            //play animation
            animator.Play(summonAnimation);

            //SUMMON THE HORDE!
            for (int i = 0; i < activeSheepBuild.Count; i++)
            {
                activeSheepBuild[i]?.RecallSheep();
            }
            for (int i = 0; i < activeSheepRam.Count; i++)
            {
                activeSheepRam[i]?.RecallSheep();
            }
            for (int i = 0; i < activeSheepFluffy.Count; i++)
            {
                activeSheepFluffy[i]?.RecallSheep();
            }
        }
    }
    public void OnSummonSheep(InputAction.CallbackContext context)
    {
        if(context.performed && canSummonSheep)
        {
            //disallow summoning
            canSummonSheep = false;

            //get flock type
            SheepTypes flockType = currentFlockType;

            //play animation
            animator.Play(summonAnimation);

            //delete all active sheep
            for(int i = 0; i < GetCurrentSheepFlock(flockType).Count; i++)
            {
                GetCurrentSheepFlock(flockType)[i].KillSheep();
            }
            GetCurrentSheepFlock(flockType).Clear();

            int amountToSummon = GetSheepAmountToSummon(flockType);

            //summon sheep!
            for(int i = 0; i < amountToSummon; i++)
            {
                StartCoroutine(SummonSheep(flockType));
            }

            //start cooldown
            StartCoroutine(SummonSheepCooldown());
        }
    }
    IEnumerator SummonSheep(SheepTypes theSheepType)
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
            var sheep = Instantiate(GetCurrentSheepPrefab(theSheepType), summonPosition, Quaternion.identity) as GameObject;
            GetCurrentSheepFlock(theSheepType).Add(sheep.GetComponent<PlayerSheepAI>());
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
            SheepTypes flockType = currentFlockType;

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
                for(int i = 0; i < GetCurrentSheepFlock(flockType).Count; i++)
                {
                    if(GetCurrentSheepFlock(flockType)[i].IsCommandable()) GetCurrentSheepFlock(flockType)[i]?.BeginCharge(hit.point);
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

    #region Sheep Defend
    void CheckDefend()
    {
        defendPointPivot.Rotate(0f, defendPivotRotateSpeed * Time.deltaTime, 0f);
    }
    public void OnSheepDefend(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            SheepTypes flockType = currentFlockType;

            animator.Play(defendAnimation);

            int pointIndex = 0;
        
            for (int i = 0; i < GetCurrentSheepFlock(flockType).Count; i++)
            {
                 if (GetCurrentSheepFlock(flockType)[i].IsCommandable()) GetCurrentSheepFlock(flockType)[i]?.BeginDefendPlayer(defendPoints[pointIndex]);

                pointIndex++;
                if (pointIndex >= defendPoints.Count) pointIndex = 0;
            }
        }
    }
    #endregion

    #region Sheep Launch
    public void OnSheepLaunch(InputAction.CallbackContext context)
    {
        if(context.started && canLaunch)
        {
            SheepTypes flockType = currentFlockType;

            //do we have any sheep? 
            if(GetCurrentSheepFlock(flockType).Count > 0)
            {
                //are sheep nearby?
                for(int i = 0; i < GetCurrentSheepFlock(flockType).Count; i++)
                {
                    if(Vector3.Distance(transform.position, GetCurrentSheepFlock(flockType)[i].transform.position) <= minDistanceToLaunch)
                    {
                        animator.Play(launchAnimation);

                        //break loop and launch that mf
                        var launchSheep = Instantiate(launchProjectile, launchOrigin.position, launchOrigin.rotation);
                        launchSheep.GetComponent<Rigidbody>()?.AddForce(launchOrigin.transform.forward * launchForce + launchOrigin.transform.up * launchForceLift);
                        launchSheep.GetComponent<Rigidbody>()?.AddTorque(100f, 100f, 100f);

                        GetCurrentSheepFlock(flockType)[i].KillSheep();
                        GetCurrentSheepFlock(flockType).Remove(GetCurrentSheepFlock(flockType)[i]);

                        break;
                    }
                }
            }

            StartCoroutine(SheepLaunchCooldown());
        }
    }
    IEnumerator SheepLaunchCooldown()
    {
        yield return new WaitForSeconds(launchCooldown);
        canLaunch = true;
    }
    #endregion

}
