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
    [SerializeField] AudioClip abilitySound;
    [SerializeField] AudioClip launchSound;
    [SerializeField] AudioClip summonSound;
    AudioSource audioSource;

    [Header("Goth Mode ")]
    [SerializeField] GameObject gothExplosion;

    [Header("Sheep Flock Variables")]
    [SerializeField] List<PlayerSheepAI> activeSheepBuild;
    [SerializeField] List<PlayerSheepAI> activeSheepRam;
    [SerializeField] List<PlayerSheepAI> activeSheepFluffy;
    [SerializeField] GameObject sheepPrefabBuild;
    [SerializeField] GameObject sheepPrefabRam;
    [SerializeField] GameObject sheepPrefabFluffy;
    [SerializeField] SheepTypes currentFlockType;
    [SerializeField] float maxDistanceToUseAbilities = 30f;
    int currentFlockIndex;

    [Header("Sheep Flock Leaders")]
    public List<PlayerSheepAI> leaderSheep;

    [Header("Sheep Swap Variables")]
    [SerializeField] PlayerFlockSelectMenu flockSelectMenu;
    [SerializeField] float flockMenuTimescale = 0.25f;
    [SerializeField] float defaultTimescale = 1;
    bool isInFlockMenu = false;

    [Header("Sheep Summon Variables")]
    [SerializeField] float summonBloodCost = 25f;
    [SerializeField] int amountToSummonBuild;
    [SerializeField] int amountToSummonRam;
    [SerializeField] int amountToSummonFluffy;
    [SerializeField] float summonRadius = 20f;
    [SerializeField] float summonIntervalMin = 0f;
    [SerializeField] float summonIntervalMax = 0.5f;
    [SerializeField] float summonCooldown = 5f;
    [SerializeField] float summonBlackSheepPercent = 10f;
    [SerializeField] AbilityIcon summonIcon;
    [SerializeField] string summonAnimation;
    bool canSummonSheep = true;
    bool canSummonAllSheep = true;
    PlayerSummoningResource summonResource;

    [Header("Sheep Charge Variables")]
    [SerializeField] Vector3 chargePointOffset;
    [SerializeField] GameObject sheepChargePointPrefab;
    [SerializeField] LayerMask chargeTargetLayers;
    [SerializeField] string chargeAnimation;
    [SerializeField] AbilityIcon chargeIcon;
    [SerializeField] float chargeCooldown = 1f;
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
    //[SerializeField] float launchForce = 2500f;
    //[SerializeField] float launchForceLift = 250f;
    [SerializeField] List<GameObject> launchProjectiles;
    [SerializeField] Transform launchOrigin;
    [SerializeField] float minDistanceToLaunch = 10f;
    [SerializeField] string launchAnimation;
    [SerializeField] AbilityIcon launchIcon;
    [SerializeField] float launchCooldown = 1f;
    bool canLaunch = true;

    Animator animator;

    private void Start()
    {
        summonResource = GetComponent<PlayerSummoningResource>();
        animator = GetComponent<Animator>();
        currentFlockIndex = (int)currentFlockType;

        audioSource = GetComponent<AudioSource>();

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
        //if held down, open menu and slow time. if pressed, just go to next in list
        if(context.performed)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            isInFlockMenu = true;
            Debug.Log("in flock menu");

            //enable flock select menu
            Time.timeScale = flockMenuTimescale;
            Time.fixedDeltaTime = 0.02F * Time.timeScale; //evil physics timescale hack to make it smooth
            flockSelectMenu.gameObject.SetActive(true);
        }
        if (context.canceled)
        {
            if(isInFlockMenu)
            {
                currentFlockType = flockSelectMenu.flockToChangeTo;
                currentFlockIndex = (int)currentFlockType;

                Debug.Log("Current Flock is: " + currentFlockType);
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
            else
            {
                //go to next sheep type
                currentFlockIndex++;
                if (currentFlockIndex >= 3) currentFlockIndex = 0; //dont try to set to an enum that doesnt exist 
                currentFlockType = (SheepTypes)currentFlockIndex;

                Debug.Log("Current Flock is: " + currentFlockType);
                sheepTypeText.text = "Current Sheep Type: " + currentFlockType;
                sheepTypeText.color = sheepTypeColors[(int)currentFlockType];
            }
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
            //get flock type
            SheepTypes flockType = currentFlockType;

            //play animation
            animator.Play(summonAnimation);

            //TEMP SOUND
            audioSource.PlayOneShot(abilitySound);

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

            //TEMP SOUND
            audioSource.PlayOneShot(abilitySound);

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
        if(context.performed && canSummonSheep && summonResource.GetCurrentBlood() >= summonBloodCost)
        {
            summonResource.ChangeBloodAmount(-summonBloodCost);

            //disallow summoning
            canSummonSheep = false;

            //get flock type
            SheepTypes flockType = currentFlockType;

            //play animation
            animator.Play(summonAnimation);

            //delete all active sheep
            for(int i = 0; i < GetCurrentSheepFlock(flockType).Count; i++)
            {
                GetCurrentSheepFlock(flockType)[i].DestroySheep();
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

            //TEMP SOUND
            audioSource.PlayOneShot(summonSound);

            summonIcon.CooldownUIEffect(summonCooldown);
        }
    }
    public void OnSummonAllSheep(InputAction.CallbackContext context)
    {
        if (context.performed && canSummonAllSheep)
        {
            canSummonAllSheep = false;
   
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
            StartCoroutine(SummonAllSheepCooldown());

            //TEMP SOUND
            audioSource.PlayOneShot(summonSound);
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
            if (rand <= summonBlackSheepPercent) sheep.GetComponent<PlayerSheepAI>().isBlackSheep = true;
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

    #region Sheep Charge
    public void OnSheepCharge(InputAction.CallbackContext context)
    {
        if(canCharge)
        {
            if (context.started)
            {
                //spawn icon
                var chargePoint = Instantiate(sheepChargePointPrefab, transform.position, Quaternion.identity) as GameObject;
                sheepChargePoint = chargePoint;

                //prepare to charge
                isPreparingCharge = true;
            }

            if (context.canceled)
            {
                SheepTypes flockType = currentFlockType;

                //stop charging
                isPreparingCharge = false;

                //play animation
                animator.Play(chargeAnimation);

                //TEMP SOUND
                audioSource.PlayOneShot(abilitySound);

                //get rid of icon
                Destroy(sheepChargePoint);

                if (CheckIfCloseToLeader(flockType))
                {
                    //send sheep to point if valid!
                    RaycastHit hit;
                    if (Physics.Raycast(Camera.main.transform.position + chargePointOffset, Camera.main.transform.forward, out hit, Mathf.Infinity, chargeTargetLayers))
                    {
                        for (int i = 0; i < GetCurrentSheepFlock(flockType).Count; i++)
                        {
                            if (GetCurrentSheepFlock(flockType)[i].IsCommandable()) GetCurrentSheepFlock(flockType)[i]?.BeginCharge(hit.point);
                        }
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
                SheepTypes flockType = currentFlockType;

                //if close to flock leader, defense mode activate
                if (CheckIfCloseToLeader(flockType))
                {
                    animator.Play(defendAnimation);

                    //TEMP SOUND
                    audioSource.PlayOneShot(abilitySound);

                    int pointIndex = 0;

                    for (int i = 0; i < GetCurrentSheepFlock(flockType).Count; i++)
                    {
                        if (GetCurrentSheepFlock(flockType)[i].IsCommandable()) GetCurrentSheepFlock(flockType)[i]?.BeginDefendPlayer(defendPoints[pointIndex]);

                        pointIndex++;
                        if (pointIndex >= defendPoints.Count) pointIndex = 0;
                    }
                }
                //else do a boo womp sound D:

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

                        //TEMP SOUND
                        audioSource.PlayOneShot(launchSound);

                        //break loop and launch that mf
                        var launchSheep = Instantiate(launchProjectiles[(int)currentFlockType], launchOrigin.position, launchOrigin.rotation);
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
