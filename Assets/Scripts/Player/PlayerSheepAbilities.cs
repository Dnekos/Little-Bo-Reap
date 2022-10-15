using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using TMPro;

public enum SheepTypes
{
    BUILD = 0,
    RAM = 1,
    FLUFFY = 2
}
public class PlayerSheepAbilities : MonoBehaviour
{
    [Header("UI Test")]
    [SerializeField] TextMeshProUGUI sheepTypeText;
    [SerializeField] List<Color> sheepTypeColors;

    [Header("Temp Sounds")]
    [SerializeField] FMODUnity.EventReference abilitySound;
    [SerializeField] FMODUnity.EventReference launchSound;
    [SerializeField] FMODUnity.EventReference summonSound;

    [Header("Camera Access")]
    [SerializeField] PlayerCameraFollow playerCam;

    [Header("Goth Mode")]
    [SerializeField] GameObject gothExplosion;
    [SerializeField] PlayerGothMode gothMode;

    [Header("Sheep Flock Variables")]
    [SerializeField] List<PlayerSheepAI> activeSheepBuild;
    [SerializeField] List<PlayerSheepAI> activeSheepRam;
    [SerializeField] List<PlayerSheepAI> activeSheepFluffy;
    [SerializeField] GameObject sheepPrefabBuild;
    [SerializeField] GameObject sheepPrefabRam;
    [SerializeField] GameObject sheepPrefabFluffy;
    public SheepTypes currentFlockType;
    [SerializeField] float maxDistanceToUseAbilities = 30f;
    int currentFlockIndex;

    [Header("Sheep Flock Leaders")]
    public List<PlayerSheepAI> leaderSheep;

    [Header("Sheep Swap Variables")]
    [SerializeField] PlayerFlockSelectMenu flockSelectMenu;
    [SerializeField] float flockMenuTimescale = 0.25f;
    [SerializeField] float defaultTimescale = 1;
    bool isInFlockMenu = false;
    float swapContextValue;

    [Header("Sheep Summon Variables")]
    [SerializeField] float summonBloodCost = 25f;
    [SerializeField] int amountToSummonBuild;
    [SerializeField] int amountToSummonRam;
    [SerializeField] int amountToSummonFluffy;
    [SerializeField] float summonRadius = 20f;
    [SerializeField] float summonIntervalMin = 0f;
    [SerializeField] float summonIntervalMax = 0.5f;
    [SerializeField] float summonCooldown = 5f;
    [Range(0f, 100f)]
    [SerializeField] float summonBlackSheepPercent = 10f;
    [SerializeField] AbilityIcon summonIcon;
    [SerializeField] string summonAnimation;
	[SerializeField] ParticleSystem RecallVFX;
    bool canSummonSheep = true;
    bool canSummonAllSheep = true;
    PlayerSummoningResource summonResource;
    bool summonPerformed = false;
    bool recallPerformed = false;

    [Header("Sheep Attack Variables")]
    [SerializeField] Vector3 attackPointOffset;
    [SerializeField] GameObject sheepAttackPointPrefab;
    [SerializeField] LayerMask attackTargetLayers;
    [SerializeField] string attackAnimation;
    [SerializeField] AbilityIcon attackIcon;
    [SerializeField] float attackCooldown = 1f;
    [SerializeField] float attackRadius = 10f;
    bool canAttack = true;
    GameObject sheepAttackPoint;
    Vector3 attackPosition;
    bool isPreparingAttack;

    [Header("Sheep Charge Variables")]
    [SerializeField] Vector3 chargePointOffset;
    [SerializeField] GameObject sheepChargePointPrefab;
    [SerializeField] LayerMask chargeTargetLayers;
    [SerializeField] string chargeAnimation;
    [SerializeField] AbilityIcon chargeIcon;
    [SerializeField] float chargeCooldown = 1f;
    [SerializeField] float chargeDistanceToUse;
    bool canCharge = true;
    GameObject sheepChargePoint;
    Vector3 chargePosition;
    bool isPreparingCharge;

    [Header("Sheep Defend Variables")]
    [SerializeField] List<Transform> defendPoints;
    [SerializeField] Transform defendPointPivot;
    [SerializeField] float defendPivotRotateSpeed = 1f;
    [SerializeField] float defendDistance = 3f;
    [SerializeField] string defendAnimation;
    [SerializeField] AbilityIcon defendIcon;
    [SerializeField] float defendCooldown = 1f;
    bool canDefend = true;

    [Header("Sheep Launch Variables")]

    [SerializeField] List<GameObject> launchProjectiles;
    [SerializeField] Transform launchOrigin;
    [SerializeField] float minDistanceToLaunch = 10f;
    [SerializeField] string launchAnimation;
    [SerializeField] AbilityIcon launchIcon;
    [SerializeField] float launchCooldown = 1f;
    bool canLaunch = true;
    bool isLaunching = false;

    Animator animator;

    private void Start()
    {
        summonResource = GetComponent<PlayerSummoningResource>();
        animator = GetComponent<PlayerAnimationController>().playerAnimator;
        currentFlockIndex = (int)currentFlockType;
        gothMode = GetComponent<PlayerGothMode>();

        //set defend distance of each defence point
        for(int i = 0; i < defendPoints.Count; i++)
        {
            defendPoints[i].position += defendPoints[i].forward * defendDistance;
        }
    }
    private void Update()
    {
        CheckAttack();
        CheckCharge();
        CheckDefend();
        CheckLaunch();
    }

    #region Sheep Flock Functions
    public void OnChangeSheepFlock(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            swapContextValue = context.ReadValue<float>(); 
            //Debug.Log(swapContextValue);
        }
        //alright so if we press middle mouse down, context value is one
        //if useing scroll wheel, its -240 and 240 respectively for some reason
        //so use these magic numbers to determine what to do

        //if held down, open menu and slow time. if pressed, just go to next in list
        if(context.performed && swapContextValue == 1)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            isInFlockMenu = true;
            //Debug.Log("in flock menu");

            //enable flock select menu
            Time.timeScale = flockMenuTimescale;
            Time.fixedDeltaTime = 0.02F * Time.timeScale; //evil physics timescale hack to make it smooth
            flockSelectMenu.gameObject.SetActive(true);
        }
        if (context.canceled && swapContextValue == 1)
        {
            if(isInFlockMenu)
            {
                currentFlockType = flockSelectMenu.flockToChangeTo;
                currentFlockIndex = (int)currentFlockType;

                //Debug.Log("Current Flock is: " + currentFlockType);
                sheepTypeText.text = "Current Sheep Type: " + currentFlockType;
                sheepTypeText.color = sheepTypeColors[(int)currentFlockType];

                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

                //disable flock select menu
                isInFlockMenu = false;
                Time.timeScale = defaultTimescale;
                Time.fixedDeltaTime = 0.02F; //evil physics timescale hack
                flockSelectMenu.gameObject.SetActive(false);
            }      
        }
        if(context.performed && swapContextValue > 1)
        {
            //go to next sheep type
            currentFlockIndex++;
            if (currentFlockIndex >= 3) currentFlockIndex = 0; //dont try to set to an enum that doesnt exist 
            currentFlockType = (SheepTypes)currentFlockIndex;

            //Debug.Log("Current Flock is: " + currentFlockType);
            sheepTypeText.text = "Current Sheep Type: " + currentFlockType;
            sheepTypeText.color = sheepTypeColors[(int)currentFlockType];
        }
        else if(context.performed && swapContextValue < -1)
        {
            //go to previous sheep type
            currentFlockIndex--;
            if (currentFlockIndex < 0) currentFlockIndex = 2; //dont try to set to an enum that doesnt exist 
            currentFlockType = (SheepTypes)currentFlockIndex;

            //Debug.Log("Current Flock is: " + currentFlockType);
            sheepTypeText.text = "Current Sheep Type: " + currentFlockType;
            sheepTypeText.color = sheepTypeColors[(int)currentFlockType];
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

    public void RemoveSheepFromList(SheepTypes theType, PlayerSheepAI theSheep)
    {
        GetCurrentSheepFlock(theType).Remove(theSheep);
    }

    bool CheckIfCloseToLeader(SheepTypes theSheepType)
    {
        //make sure you have a flock!
        if (GetCurrentSheepFlock(theSheepType).Count > 0)
        {
            float checkDistance = Vector3.Distance(transform.position, leaderSheep[(int)theSheepType].transform.position);

            //if you're close to a given leader, you can use an ability.
            if (checkDistance <= maxDistanceToUseAbilities) return true;
            else return false;
        }
        else return false;
    }

    #endregion

    #region Goth Mode
    public void GoGothMode()
    {
        playerCam.ShakeCamera(true);

        //turn each sheep into a black sheep
        for (int i = 0; i < 3; i++) 
        {
            for (int j = 0; j < GetCurrentSheepFlock((SheepTypes)i).Count; j++)
            {
                GetCurrentSheepFlock((SheepTypes)i)[j].GothMode();
                Instantiate(gothExplosion, GetCurrentSheepFlock((SheepTypes)i)[j].transform.position, Quaternion.identity);
            }
        }
    }

    #endregion

    #region Sheep Summon and Recall
    public void OnRecallSheep(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            //Debug.Log("performed recall");
            recallPerformed = false;
        }

        if (context.performed)
        {
            recallPerformed = true;
            //play animation
            animator.Play(summonAnimation);


            //TEMP SOUND
            FMODUnity.RuntimeManager.PlayOneShotAttached(abilitySound, gameObject);

            RecallVFX.Stop();
            RecallVFX.Play();

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

        if (context.canceled && !recallPerformed)
        {
            //get flock type
            SheepTypes flockType = currentFlockType;

            //play animation
            animator.Play(summonAnimation);

            //TEMP SOUND
            FMODUnity.RuntimeManager.PlayOneShotAttached(abilitySound, gameObject);
            RecallVFX.Stop();
            RecallVFX.Play();

            //recall current flock!
            for (int i = 0; i < GetCurrentSheepFlock(flockType).Count; i++)
            {
                GetCurrentSheepFlock(flockType)[i]?.RecallSheep();
            }
        }
    }

    public void OnSummonSheep(InputAction.CallbackContext context)
    {
        if (context.started) summonPerformed = false;

        if(context.performed && canSummonSheep && summonResource.GetCurrentBlood() >= summonBloodCost * 3)
        {
            summonPerformed = true;

            summonResource.ChangeBloodAmount(-summonBloodCost * 3);

            //disallow summoning
            canSummonSheep = false;

            //play animation
            animator.Play(summonAnimation);

            //delete all sheep
            for (int i = 0; i < 3; i++) //for each sheep type, delete list and clear it
            {
                //delete all active sheep
                for (int j = 0; j < GetCurrentSheepFlock((SheepTypes)i).Count; j++)
                {
                    GetCurrentSheepFlock((SheepTypes)i)[j].DestroySheep();
                }
                GetCurrentSheepFlock((SheepTypes)i).Clear();
            }

            //SUMMON THE HORDE
            for (int i = 0; i < 3; i++) //for each sheep type, delete list and clear it
            {
                for (int j = 0; j < GetSheepAmountToSummon((SheepTypes)i); j++)
                {
                    StartCoroutine(SummonSheep((SheepTypes)i));
                }

            }


            //start cooldown
            StartCoroutine(SummonSheepCooldown());

            //TEMP SOUND
            FMODUnity.RuntimeManager.PlayOneShotAttached(summonSound, gameObject);

            summonIcon.CooldownUIEffect(summonCooldown);
        }

        //summon 1 flock
        if(context.canceled && !summonPerformed && canSummonSheep && summonResource.GetCurrentBlood() >= summonBloodCost)
        {
            summonResource.ChangeBloodAmount(-summonBloodCost);

            //disallow summoning
            canSummonSheep = false;

            //get flock type
            SheepTypes flockType = currentFlockType;

            //play animation
            animator.Play(summonAnimation);

            // remove list slots that are null (dead sheep)
            GetCurrentSheepFlock(flockType).RemoveAll(x => x == null);

            //delete all active sheep
            for (int i = 0; i < GetCurrentSheepFlock(flockType).Count; i++)
            {
                GetCurrentSheepFlock(flockType)[i].DestroySheep();
            }
            GetCurrentSheepFlock(flockType).Clear();

            int amountToSummon = GetSheepAmountToSummon(flockType);

            //summon sheep!
            for (int i = 0; i < amountToSummon; i++)
            {
                StartCoroutine(SummonSheep(flockType));
            }

            //start cooldown
            StartCoroutine(SummonSheepCooldown());

            //TEMP SOUND
            FMODUnity.RuntimeManager.PlayOneShotAttached(summonSound, gameObject);

            summonIcon.CooldownUIEffect(summonCooldown);
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

            //determine if it's a black sheep
            float rand = Random.Range(0f, 100f);
            if (rand <= summonBlackSheepPercent || gothMode.isGothMode) sheep.GetComponent<PlayerSheepAI>().isBlackSheep = true;
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
    IEnumerator SummonAllSheepCooldown()
    {
        yield return new WaitForSeconds(summonCooldown);
        canSummonAllSheep = true;
    }
    #endregion

    #region Sheep Attack
    public void OnSheepAttack(InputAction.CallbackContext context)
    {
        if (canCharge)
        {
            SheepTypes flockType = currentFlockType;

            if (context.started)
            {
                //spawn icon
                var attackPoint = Instantiate(sheepAttackPointPrefab, transform.position, Quaternion.identity) as GameObject;
                sheepAttackPoint = attackPoint;

                //prepare to charge
                isPreparingAttack = true;
            }

            if (context.canceled)
            {
                //stop charging
                isPreparingAttack = false;

                //play animation
                animator.Play(attackAnimation);

                //TEMP SOUND
                FMODUnity.RuntimeManager.PlayOneShotAttached(abilitySound, gameObject);

                //get rid of icon
                Destroy(sheepAttackPoint);

                //send sheep to point if valid!
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.transform.position + attackPointOffset, Camera.main.transform.forward, out hit, Mathf.Infinity, attackTargetLayers))
                {
                    for (int i = 0; i < GetCurrentSheepFlock(flockType).Count; i++)
                    {
                        if (GetCurrentSheepFlock(flockType)[i].IsCommandable()) GetCurrentSheepFlock(flockType)[i]?.CreateListOfAttackTargets(hit.point, attackRadius);
                    }
                }
               

                //start cooldown
                canAttack = false;
                attackIcon.CooldownUIEffect(attackCooldown);
                StartCoroutine(AttackCooldown());

            }
        }

    }
    void CheckAttack()
    {
        if (isPreparingAttack)
        {
            //draw ray from camera forward to point
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position + attackPointOffset, Camera.main.transform.forward, out hit, Mathf.Infinity, attackTargetLayers))
            {
                //draw charge point
                sheepAttackPoint.transform.position = hit.point;
            }
            else
            {
                //draw it way the fuck down so it isnt seen
                sheepAttackPoint.transform.position = new Vector3(0f, -1000f, 0f);
            }

        }
    }

    IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
    #endregion

    #region Sheep Charge
    public void OnSheepCharge(InputAction.CallbackContext context)
    {
        if(canCharge)
        {
            //SheepTypes flockType = currentFlockType;
            SheepTypes flockType = SheepTypes.RAM;

            if (context.started)
            {
                //spawn icon
                var chargePoint = Instantiate(sheepChargePointPrefab, transform.position, Quaternion.identity) as GameObject;
                sheepChargePoint = chargePoint;

                //prepare to charge
                isPreparingCharge = true;

                //get the sheep to move to the player
                //recall current flock!
                for (int i = 0; i < GetCurrentSheepFlock(flockType).Count; i++)
                {
                    GetCurrentSheepFlock(flockType)[i]?.RecallSheep();
                }
            }

            if (context.canceled)
            {
                //stop charging
                isPreparingCharge = false;

                //play animation
                animator.Play(chargeAnimation);

                //TEMP SOUND
                FMODUnity.RuntimeManager.PlayOneShotAttached(abilitySound,gameObject);

                //get rid of icon
                Destroy(sheepChargePoint);

                
                //send sheep to point if valid!
                RaycastHit hit;
                if (Physics.Raycast(Camera.main.transform.position + chargePointOffset, Camera.main.transform.forward, out hit, Mathf.Infinity, chargeTargetLayers))
                {
                    for (int i = 0; i < GetCurrentSheepFlock(flockType).Count; i++)
                    {
                        if (GetCurrentSheepFlock(flockType)[i].IsCommandable() && Vector3.Distance(transform.position, GetCurrentSheepFlock(flockType)[i].transform.position) <= chargeDistanceToUse ) GetCurrentSheepFlock(flockType)[i]?.BeginCharge(hit.point);
                    }
                }
                

                //start cooldown
                canCharge = false;
                chargeIcon.CooldownUIEffect(chargeCooldown);
                StartCoroutine(ChargeCooldown());
            }
        }      
    }
    void CheckCharge()
    {
        if(isPreparingCharge)
        {
            //draw ray from camera forward to point
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position + chargePointOffset, Camera.main.transform.forward, out hit, Mathf.Infinity, chargeTargetLayers))
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

    IEnumerator ChargeCooldown()
    {
        yield return new WaitForSeconds(chargeCooldown);
        canCharge = true;
    }
    #endregion

    #region Sheep Defend
    void CheckDefend()
    {
        defendPointPivot.Rotate(0f, defendPivotRotateSpeed * Time.deltaTime, 0f);
    }
    public void OnSheepDefend(InputAction.CallbackContext context)
    {
        if(canDefend)
        {
            if (context.started)
            {
                //SheepTypes flockType = currentFlockType;
                //switching to be only useable by fluffy sheep, keep same architecture in case we change our minds (we probably won't)
                SheepTypes flockType = SheepTypes.FLUFFY;

                
                animator.Play(defendAnimation);


                //TEMP SOUND
                FMODUnity.RuntimeManager.PlayOneShotAttached(abilitySound,gameObject);

                int pointIndex = 0;

                for (int i = 0; i < GetCurrentSheepFlock(flockType).Count; i++)
                {
                    if (GetCurrentSheepFlock(flockType)[i].IsCommandable()) GetCurrentSheepFlock(flockType)[i]?.BeginDefendPlayer(defendPoints[pointIndex]);

                    pointIndex++;
                    if (pointIndex >= defendPoints.Count) pointIndex = 0;
                }
              
                //start cooldown
                canDefend = false;
                defendIcon.CooldownUIEffect(defendCooldown);
                StartCoroutine(DefendCooldown());

            }
        } 
    }

    IEnumerator DefendCooldown()
    {
        yield return new WaitForSeconds(chargeCooldown);
        canDefend = true;
    }
    #endregion

    #region Sheep Launch
    public void OnSheepLaunch(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            isLaunching = true;
        }
        if(context.canceled)
        {
            isLaunching = false;
        }


        //old implementation
       //if(context.started && canLaunch)
       //{
       //    SheepTypes flockType = currentFlockType;
       //
       //    //do we have any sheep? 
       //    if(GetCurrentSheepFlock(flockType).Count > 0)
       //    {
       //        //are sheep nearby?
       //        for(int i = 0; i < GetCurrentSheepFlock(flockType).Count; i++)
       //        {
       //            if(Vector3.Distance(transform.position, GetCurrentSheepFlock(flockType)[i].transform.position) <= minDistanceToLaunch)
       //            {
       //                animator.Play(launchAnimation);
       //
       //                //TEMP SOUND
       //                FMODUnity.RuntimeManager.PlayOneShotAttached(launchSound,gameObject);
       //
       //                //break loop and launch that mf
       //                var launchSheep = Instantiate(launchProjectiles[(int)currentFlockType], launchOrigin.position, launchOrigin.rotation);
       //                if (GetCurrentSheepFlock(flockType)[i].isBlackSheep) launchSheep.GetComponent<PlayerSheepProjectile>().isBlackSheep = true;
       //                launchSheep.GetComponent<PlayerSheepProjectile>().LaunchProjectile();
       //                //launchSheep.GetComponent<Rigidbody>()?.AddForce(launchOrigin.transform.forward * launchForce + launchOrigin.transform.up * launchForceLift);
       //                //launchSheep.GetComponent<Rigidbody>()?.AddTorque(100f, 100f, 100f);
       //
       //                GetCurrentSheepFlock(flockType)[i].KillSheep();
       //                //GetCurrentSheepFlock(flockType).Remove(GetCurrentSheepFlock(flockType)[i]);
       //
       //                break;
       //            }
       //        }
       //    }
       //
       //    //start cooldown
       //    canLaunch = false;
       //    launchIcon.CooldownUIEffect(launchCooldown);
       //    StartCoroutine(SheepLaunchCooldown());
       //}
    }
    void CheckLaunch()
    {
        if (isLaunching && canLaunch)
        {
            SheepTypes flockType = currentFlockType;

            //do we have any sheep? 
            if (GetCurrentSheepFlock(flockType).Count > 0)
            {
                //are sheep nearby?
                for (int i = 0; i < GetCurrentSheepFlock(flockType).Count; i++)
                {
                    if (Vector3.Distance(transform.position, GetCurrentSheepFlock(flockType)[i].transform.position) <= minDistanceToLaunch)
                    {
                        animator.Play(launchAnimation);

                        //TEMP SOUND
                        FMODUnity.RuntimeManager.PlayOneShotAttached(launchSound, gameObject);

                        //break loop and launch that mf
                        var launchSheep = Instantiate(launchProjectiles[(int)currentFlockType], launchOrigin.position, launchOrigin.rotation);
                        if (GetCurrentSheepFlock(flockType)[i].isBlackSheep) launchSheep.GetComponent<PlayerSheepProjectile>().isBlackSheep = true;
                        launchSheep.GetComponent<PlayerSheepProjectile>().LaunchProjectile();
                        //launchSheep.GetComponent<Rigidbody>()?.AddForce(launchOrigin.transform.forward * launchForce + launchOrigin.transform.up * launchForceLift);
                        //launchSheep.GetComponent<Rigidbody>()?.AddTorque(100f, 100f, 100f);

                        GetCurrentSheepFlock(flockType)[i].KillSheep();
                        //GetCurrentSheepFlock(flockType).Remove(GetCurrentSheepFlock(flockType)[i]);

                        break;
                    }
                }
            }

            //start cooldown
            canLaunch = false;
            launchIcon.CooldownUIEffect(launchCooldown);
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
