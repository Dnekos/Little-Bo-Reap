using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using TMPro;
using UnityEngine.UI;

public enum SheepTypes
{
    BUILD = 0,
    RAM = 1,
    FLUFFY = 2
}

[System.Serializable]
public struct Flock
{
	public string name;
	public List<PlayerSheepAI> activeSheep;
	public int MaxSize;
	public GameObject SheepPrefab;
	public GameObject SheepProjectilePrefab;
	public Color UIColor;
    public Sprite sheepIcon;
    public ParticleSystem spellParticle;
    public ParticleSystem flockChangeParticle;
}

public class PlayerSheepAbilities : MonoBehaviour
{
    [Header("UI Test")]
    [SerializeField] TextMeshProUGUI sheepTypeText;
    [SerializeField] TextMeshProUGUI redText;
    [SerializeField] TextMeshProUGUI flockNumber;
    [SerializeField] Image flockTypeIcon;

    [Header("Sounds")]
    [SerializeField] FMODUnity.EventReference abilitySound;
    [SerializeField] FMODUnity.EventReference summonSound;

    [Header("Camera Access")]
    [SerializeField] PlayerCameraFollow playerCam;

    [Header("Goth Mode")]
    [SerializeField] GameObject gothExplosion;
    public PlayerGothMode gothMode;

	[Header("Sheep Flock Variables")]
	public Flock[] sheepFlocks;
    public SheepTypes currentFlockType;
    [SerializeField] float maxDistanceToUseAbilities = 30f;
    int currentFlockIndex;
	[SerializeField] GameEvent respawnEvent;

    [Header("Sheep Flock Leaders")]
    public List<PlayerSheepAI> leaderSheep;

	[Header("Sheep Swap Variables")]
	[SerializeField] GameObject flockSelectMenu;
	[SerializeField] TextMeshProUGUI[] flockSelectTexts;
    [SerializeField] float flockMenuTimescale = 0.25f;
    [SerializeField] float defaultTimescale = 1;
    [SerializeField] ParticleSystem bellParticles;
    [SerializeField] ParticleSystem bellParticleBurst;
    [SerializeField] Animator SwapUIAnimator;
    [SerializeField] string swapAnimationUI;
    [SerializeField] string noSheepAnimUI;
	Vector2 WheelOpenMousePos;
    bool isInFlockMenu = false;
    float swapContextValue; // i feel like there is a way to not have this non-local

    [Header("Sheep Summon Variables")]
    [SerializeField] float summonBloodCost = 25f;
    [SerializeField] float summonRadius = 20f;
    [SerializeField] float summonCooldown = 5f;
    [Range(0f, 100f)]
    public float summonBlackSheepPercent = 10f;
    [SerializeField] string summonAnimation, racallAnimation;
	[SerializeField] ParticleSystem RecallVFX;
    [SerializeField] GameObject summonParticle;
    [SerializeField] float summonParticleLerpSpeed = 5f;
	[SerializeField] GameEvent endConstructsEvent;
	List<GameObject> spawnParticles;
    bool canSummonSheep = true;
    //bool canSummonAllSheep = true;
    PlayerSummoningResource summonResource;
    bool summonPerformed = false;
    bool recallPerformed = false;

    [Header("Sheep Attack Variables")]
    [SerializeField] Vector3 attackPointOffset;
    [SerializeField] GameObject sheepAttackPointPrefab;
    [SerializeField] GameObject sheepAttackConfirmPrefab;
    [SerializeField] LayerMask attackTargetLayers;
    [SerializeField] string attackAnimation;
    [SerializeField] AbilityIcon attackIcon;
    [SerializeField] float attackCooldown = 1f;
    [SerializeField] float attackRadius = 10f;
    bool canAttack = true;
    GameObject sheepAttackPoint;
    Vector3 attackPosition;
    bool isPreparingAttack;

    [Header("Sheep Stampede Variables")]
    [SerializeField] Vector3 chargePointOffset;
	[SerializeField] GameObject sheepChargePointPrefab;
    [SerializeField] GameObject sheepChargeConfirmPrefab;
    [SerializeField] LayerMask chargeTargetLayers;
    [SerializeField] string chargeAnimation;
    [SerializeField] AbilityIcon chargeIcon;
    [SerializeField] float chargeCooldown = 1f;
    [SerializeField] float chargeDistanceToUse;
    bool canCharge = true;
    GameObject sheepChargePoint;
    Vector3 chargePosition;
    bool isPreparingCharge;
    bool hasCharged;

    [Header("Sheep Vortex Variables")]
    [SerializeField] List<Transform> defendPoints;
    [SerializeField] Transform defendPointPivot;
    [SerializeField] float defendPivotRotateSpeed = 1f;
    [SerializeField] float defendPivotRotateSpeedGain = 5f; //this variable name is too long. too bad!
    float defendRotateBaseSpeed;
    [SerializeField] float defendDistance = 3f;
    [SerializeField] float defendTimeMax = 6f;
    float currentDefendTime = 0;
    [SerializeField] string defendAnimation;
	[SerializeField] FMODUnity.StudioEventEmitter vortexStartEmitter;
	[SerializeField] FMODUnity.EventReference vortexEndSound;
	[SerializeField] AbilityIcon defendIcon;
    [SerializeField] float defendCooldown = 1f;
    bool canDefend = true;
    bool isDefending = false;

    [Header("Sheep Launch Variables")]

    [SerializeField] Transform launchOrigin;
    [SerializeField] float minDistanceToLaunch = 10f;
    [SerializeField] string launchAnimation;
    [SerializeField] AbilityIcon launchIcon;
    [SerializeField] float launchCooldown = 1f;
    int flockIndex;
    int sheepIndex;
    bool canLaunch = true;
    bool isLaunching = false;

    Animator animator;

    private void Start()
    {
		// needed to prevent gamepad from opening debuggers, comment out if you want the SRP debug window
		UnityEngine.Rendering.DebugManager.instance.enableRuntimeUI = false;

		respawnEvent.listener.AddListener(DeleteAllSheep);

		summonResource = GetComponent<PlayerSummoningResource>();
        animator = GetComponent<PlayerAnimationController>().playerAnimator;
        currentFlockIndex = (int)currentFlockType;
        gothMode = GetComponent<PlayerGothMode>();
		spawnParticles = new List<GameObject>();
		//set defend distance of each defence point
		for (int i = 0; i < defendPoints.Count; i++)
        {
            defendPoints[i].position += defendPoints[i].forward * defendDistance;
        }

        defendRotateBaseSpeed = defendPivotRotateSpeed;

        UpdateFlockUI();
    }
    private void Update()
    {
		// set leader
		for (int i = 0; i < 3; i++)
			for (int j = 0; j < sheepFlocks[i].activeSheep.Count; j++)
				sheepFlocks[i].activeSheep[j].leaderSheep = sheepFlocks[i].activeSheep[0];

        CheckAttack();
        CheckCharge();
        CheckDefend();
        CheckLaunch();
    }

	#region Sheep Flock Functions
	public void DeleteAllSheep()
	{
		for (int i = 0; i < 3; i++) //for each sheep type, delete list and clear it
		{
			//delete all active sheep
			for (int j = sheepFlocks[i].activeSheep.Count - 1; j >= 0; j--)
			{
                sheepFlocks[i].activeSheep[j].GibSheep();
				sheepFlocks[i].activeSheep[j].KillSheep();
			}
			sheepFlocks[i].activeSheep.Clear();
		}
	}
	public void DeleteFlock(SheepTypes sheep)
	{
		int i = (int)sheep;
		//delete all active sheep
		for (int j = sheepFlocks[i].activeSheep.Count - 1; j >= 0; j--)
		{
			sheepFlocks[i].activeSheep[j].KillSheep();
		}
		sheepFlocks[i].activeSheep.Clear();
	}
	public float GetAverageActiveFlockSize()
	{
		return (sheepFlocks[0].activeSheep.Count + 
				sheepFlocks[1].activeSheep.Count + 
				sheepFlocks[2].activeSheep.Count) * 0.33f;
	}
	public float GetAverageMaxFlockSize()
	{
		return (sheepFlocks[0].MaxSize +
				sheepFlocks[1].MaxSize +
				sheepFlocks[2].MaxSize) * 0.33f;
	}

	public void OnSetActiveFlock(InputAction.CallbackContext context)
	{
		int newindex = Mathf.FloorToInt(context.ReadValue<float>()) - 1;
		Debug.Log(newindex);
		// hold onto the original index, so that we can exit the loop if we are back to the start (should only happen if all maxs are 0)
		if (context.started && sheepFlocks[newindex].MaxSize > 0 && newindex != currentFlockIndex)
		{
			currentFlockIndex = newindex;
			currentFlockType = (SheepTypes)currentFlockIndex;

			sheepTypeText.text = "Current Sheep Type: " + currentFlockType;
			sheepTypeText.color = sheepFlocks[currentFlockIndex].UIColor;

			var particleModule = bellParticles.main;
			particleModule.startColor = sheepFlocks[currentFlockIndex].UIColor;
			sheepFlocks[currentFlockIndex].flockChangeParticle.Play(true);

			if (SwapUIAnimator.gameObject.activeSelf)
				SwapUIAnimator.Play(swapAnimationUI);

			UpdateFlockUI();
		}
    }


	public void OnChangeSheepFlock(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			int originalIndex = currentFlockIndex;
			do
				// go to next sheep type. Mod keep it in the rand of 0-2
				currentFlockIndex = Mod(currentFlockIndex + (int)Mathf.Sign(swapContextValue), sheepFlocks.Length);
			while (sheepFlocks[currentFlockIndex].MaxSize <= 0 && originalIndex != currentFlockIndex);

			currentFlockType = (SheepTypes)currentFlockIndex;

			sheepTypeText.text = "Current Sheep Type: " + currentFlockType;
			sheepTypeText.color = sheepFlocks[currentFlockIndex].UIColor;

			var particleModule = bellParticles.main;
			particleModule.startColor = sheepFlocks[currentFlockIndex].UIColor;
			sheepFlocks[currentFlockIndex].flockChangeParticle.Play(true);

			if (SwapUIAnimator.gameObject.activeSelf)
				SwapUIAnimator.Play(swapAnimationUI);

			UpdateFlockUI();
		}
		
	}

	#region depricated
	public void OldOnChangeSheepFlock(InputAction.CallbackContext context)
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
		if (context.performed && swapContextValue == 1)
		{
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;

			isInFlockMenu = true;

			//enable flock select menu
			WheelOpenMousePos = Mouse.current.position.ReadValue();

			Time.timeScale = flockMenuTimescale;
			Time.fixedDeltaTime = 0.02F * Time.timeScale; //evil physics timescale hack to make it smooth
			flockSelectMenu.gameObject.SetActive(true);
		}
		else if (context.performed)
		{
			// hold onto the original index, so that we can exit the loop if we are back to the start (should only happen if all maxs are 0)
			int originalIndex = currentFlockIndex;
			do
				// go to next sheep type. Mod keep it in the rand of 0-2
				currentFlockIndex = Mod(currentFlockIndex + (int)Mathf.Sign(swapContextValue), sheepFlocks.Length);
			while (sheepFlocks[currentFlockIndex].MaxSize <= 0 && originalIndex != currentFlockIndex);

			currentFlockType = (SheepTypes)currentFlockIndex;

			sheepTypeText.text = "Current Sheep Type: " + currentFlockType;
			sheepTypeText.color = sheepFlocks[currentFlockIndex].UIColor;

            var particleModule = bellParticles.main;
            particleModule.startColor = sheepFlocks[currentFlockIndex].UIColor;
            sheepFlocks[currentFlockIndex].flockChangeParticle.Play(true);

			if (SwapUIAnimator.gameObject.activeSelf)
			SwapUIAnimator.Play(swapAnimationUI);

            UpdateFlockUI();
        }
		else if (context.canceled && swapContextValue == 1 && isInFlockMenu)
		{
			float SelectorAngle = -Vector2.SignedAngle(Vector2.up, Mouse.current.position.ReadValue() - WheelOpenMousePos);
			int flockToChange = Mod(Mathf.FloorToInt(SelectorAngle / (360 / sheepFlocks.Length)) + 1, sheepFlocks.Length);

			// only change flocks if they have valid sheep
			if (sheepFlocks[flockToChange].MaxSize > 0)
			{
				currentFlockIndex = flockToChange;
				currentFlockType = (SheepTypes)currentFlockIndex;

				//Debug.Log("Current Flock is: " + currentFlockType);
				sheepTypeText.text = "Current Sheep Type: " + currentFlockType;
				sheepTypeText.color = sheepFlocks[currentFlockIndex].UIColor;

                var particleModule = bellParticles.main;
                particleModule.startColor = sheepFlocks[currentFlockIndex].UIColor;
                sheepFlocks[currentFlockIndex].flockChangeParticle.Play(true);

				if (SwapUIAnimator.gameObject.activeSelf)
				SwapUIAnimator.Play(swapAnimationUI);

                UpdateFlockUI();
            }

			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;

			//disable flock select menu
			isInFlockMenu = false;
			Time.timeScale = defaultTimescale;
			Time.fixedDeltaTime = 0.02F; //evil physics timescale hack
			flockSelectMenu.gameObject.SetActive(false);
		}   
    }
	#endregion
	public List<PlayerSheepAI> GetSheepFlock(SheepTypes theFlockType)
    {
		return sheepFlocks[(int)theFlockType].activeSheep;
    }
    public GameObject GetCurrentSheepPrefab(SheepTypes theFlockType)
    {
		return sheepFlocks[(int)theFlockType].SheepPrefab;
    }
    public int GetSheepAmountToSummon(SheepTypes theFlockType)
    {
		return sheepFlocks[(int)theFlockType].MaxSize;
    }

    public void RemoveSheepFromList(int theType, PlayerSheepAI theSheep)
    {
        sheepFlocks[theType].activeSheep.Remove(theSheep);
        UpdateFlockUI();
    }

    public void UpdateFlockUI()
    {
		for (int i = 0; i < sheepFlocks.Length; i++)
			flockSelectTexts[i].text = sheepFlocks[i].activeSheep.Count + "/" + sheepFlocks[i].MaxSize;

		flockTypeIcon.sprite = sheepFlocks[currentFlockIndex].sheepIcon;
		flockNumber.text = flockSelectTexts[currentFlockIndex].text;
        redText.text = flockNumber.text;
        flockNumber.color = sheepFlocks[currentFlockIndex].UIColor;
	}

    bool CheckIfCloseToLeader(SheepTypes theSheepType)
    {
		// if you have a flock and you're close to a given leader, you can use an ability.
		return GetSheepFlock(theSheepType).Count > 0 && 
			Vector3.Distance(transform.position, leaderSheep[(int)theSheepType].transform.position) <= maxDistanceToUseAbilities;
    }

    #endregion

    #region Goth Mode
    public void GoGothMode()
    {
        playerCam.ShakeCamera(true);

        //turn each sheep into a black sheep
        for (int i = 0; i < 3; i++) 
        {
            for (int j = 0; j < GetSheepFlock((SheepTypes)i).Count; j++)
            {
                GetSheepFlock((SheepTypes)i)[j].GothMode();
                Instantiate(gothExplosion, GetSheepFlock((SheepTypes)i)[j].transform.position, Quaternion.identity);
            }
        }
    }

    #endregion

    #region Sheep Summon and Recall
    public void OnRecallSheep(InputAction.CallbackContext context)
    {

        if (context.started)
        {
            GetComponent<PlayerSheepLift>().CollapseTower();

            //play animation
            animator.Play(racallAnimation);

			// stop constructs
			endConstructsEvent.listener.Invoke();

			//TEMP SOUND
			FMODUnity.RuntimeManager.PlayOneShotAttached(abilitySound, gameObject);

            RecallVFX.Stop();
            RecallVFX.Play();

            //SUMMON THE HORDE!

            for (int i = 0; i < sheepFlocks.Length; i++)
                for (int j = 0; j < sheepFlocks[i].activeSheep.Count; j++)
                    sheepFlocks[i].activeSheep[j]?.RecallSheep();
        }

		#region used to be recall all on hold, now recal just recalls all sheep cause people were too confused
		//KEEP THIS HERE IN CASE WE GO BACK LATER
		//if(context.started)
		//{
		//    //Debug.Log("performed recall");
		//    recallPerformed = false;
		//}
		//
		//bool recallAll = context.canceled && !recallPerformed;
		//
		//if ((context.performed || recallAll) && sheepFlocks[currentFlockIndex].MaxSize > 0)
		//{
		//	GetComponent<PlayerSheepLift>().CollapseTower();
		//
		//    recallPerformed = true;
		//    //play animation
		//    animator.Play(summonAnimation);
		//
		//
		//    //TEMP SOUND
		//    FMODUnity.RuntimeManager.PlayOneShotAttached(abilitySound, gameObject);
		//
		//    RecallVFX.Stop();
		//    RecallVFX.Play();
		//
		//    //SUMMON THE HORDE!
		//	if (recallAll)
		//	{
		//		for (int i = 0; i < GetSheepFlock(currentFlockType).Count; i++)
		//			GetSheepFlock(currentFlockType)[i]?.RecallSheep();
		//	}
		//	else
		//	{
		//		for (int i = 0; i < sheepFlocks.Length; i++)
		//			for (int j = 0; j < sheepFlocks[i].activeSheep.Count; j++)
		//				sheepFlocks[i].activeSheep[j]?.RecallSheep();
		//	}
		//}
		#endregion
	}

	public void OnSummonSheep(InputAction.CallbackContext context)
    {
        if(context.started && canSummonSheep && sheepFlocks[currentFlockIndex].MaxSize > 0 && summonResource.GetCurrentBlood() >= summonBloodCost)
        {
            summonResource.ChangeBloodAmount(-summonBloodCost);

            //disallow summoning
            canSummonSheep = false;

            //get flock type
            SheepTypes flockType = currentFlockType;

            //play animation
            animator.Play(summonAnimation);

            sheepFlocks[(int)flockType].spellParticle.Play(true);

			// remove list slots that are null (dead sheep)
			//GetSheepFlock(flockType).RemoveAll(x => x == null);

			// stop constructs
			endConstructsEvent.listener.Invoke();

			for (int i = spawnParticles.Count - 1; i >= 0; i--)
			{
				// ignore list slots that are null 
				if (spawnParticles[i] != null && spawnParticles[i].GetComponent<Sheep_Summon_Particle>().sheepType == flockType)
				{
					Destroy(spawnParticles[i]);
				}
			}
			spawnParticles.Clear();

			//delete all active sheep
			for (int i = GetSheepFlock(flockType).Count - 1; i >= 0; i--)
            {
				// ignore list slots that are null (dead sheep)
				if (GetSheepFlock(flockType)[i] != null)
				{
					GetSheepFlock(flockType)[i].GibSheep();
					GetSheepFlock(flockType)[i].KillSheep();
				}
            }
            GetSheepFlock(flockType).Clear();

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

			//in case we summon all at once
			#region old stuff
			/*
            summonResource.ChangeBloodAmount(-summonBloodCost);
            
            //disallow summoning
            canSummonSheep = false;
            
            //play animation
            animator.Play(summonAnimation);
            
            
            // delete all sheep
            DeleteAllSheep();
            
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
			*/
		}



		//KEEP THIS IN CASE WE GO BACK LATER
		//if (context.started) summonPerformed = false;
		//
		//if(context.performed && canSummonSheep && sheepFlocks[currentFlockIndex].MaxSize > 0 && summonResource.GetCurrentBlood() >= summonBloodCost * 3)
		//{
		//    summonPerformed = true;
		//
		//    summonResource.ChangeBloodAmount(-summonBloodCost);
		//
		//    //disallow summoning
		//    canSummonSheep = false;
		//
		//    //play animation
		//    animator.Play(summonAnimation);
		//
		//	//delete all sheep
		//	DeleteAllSheep();
		//
		//    //SUMMON THE HORDE
		//    for (int i = 0; i < 3; i++) //for each sheep type, delete list and clear it
		//    {
		//        for (int j = 0; j < GetSheepAmountToSummon((SheepTypes)i); j++)
		//        {
		//            StartCoroutine(SummonSheep((SheepTypes)i));
		//        }
		//    }
		//
		//
		//    //start cooldown
		//    StartCoroutine(SummonSheepCooldown());
		//
		//    //TEMP SOUND
		//    FMODUnity.RuntimeManager.PlayOneShotAttached(summonSound, gameObject);
		//
		//    summonIcon.CooldownUIEffect(summonCooldown);
		//}
		//
		//KEEP THIS IN CASE WE GO BACK LATER
		// //summon 1 flock
		// if(context.canceled && !summonPerformed && sheepFlocks[currentFlockIndex].MaxSize > 0 && canSummonSheep && summonResource.GetCurrentBlood() >= summonBloodCost)
		// {
		//     summonResource.ChangeBloodAmount(-summonBloodCost);
		//
		//     //disallow summoning
		//     canSummonSheep = false;
		//
		//     //get flock type
		//     SheepTypes flockType = currentFlockType;
		//
		//     //play animation
		//     animator.Play(summonAnimation);
		//
		//     // remove list slots that are null (dead sheep)
		//     GetSheepFlock(flockType).RemoveAll(x => x == null);
		//
		//	//delete all active sheep
		//	for (int i = GetSheepFlock(flockType).Count - 1; i >= 0; i--)
		//     {
		//         GetSheepFlock(flockType)[i].KillSheep();
		//     }
		//     GetSheepFlock(flockType).Clear();
		//
		//     int amountToSummon = GetSheepAmountToSummon(flockType);
		//
		//     //summon sheep!
		//     for (int i = 0; i < amountToSummon; i++)
		//     {
		//         StartCoroutine(SummonSheep(flockType));
		//     }
		//
		//     //start cooldown
		//     StartCoroutine(SummonSheepCooldown());
		//
		//     //TEMP SOUND
		//     FMODUnity.RuntimeManager.PlayOneShotAttached(summonSound, gameObject);
		//
		//     summonIcon.CooldownUIEffect(summonCooldown);
		// }
		#endregion
	}

	IEnumerator SummonSheep(SheepTypes theSheepType)
    {
        yield return null;

		int count = 0;
		bool found = false;
		NavMeshHit hit;
		Vector3 randomPosition;
		NavMeshPath path = new NavMeshPath();
		do
		{
			//get random point inside summoning radius
			Vector3 summonPosition = Vector3.zero;
			randomPosition = Random.insideUnitSphere * summonRadius;
			randomPosition += transform.position;
			if (NavMesh.SamplePosition(randomPosition, out hit, summonRadius, 1) && NavMesh.SamplePosition(transform.position, out NavMeshHit start, summonRadius, 1))
			{
				if (start.position != Vector3.positiveInfinity && hit.position != Vector3.positiveInfinity)
				{
					if (NavMesh.CalculatePath(start.position, hit.position, 1, path) && path.corners.Length > 0)
					{
						found = path.corners[0] == start.position;
						found &= path.corners[path.corners.Length - 1] == hit.position;

					}
				}
			}
			
		} while (++count < 20 && !found);

        

        //if inside navmesh, spawn sheep!
        if (found)
        {
			Debug.Log(hit.position + " " + path.corners[path.corners.Length - 1]);

			var soulParticle = Instantiate(summonParticle, transform.position, Quaternion.identity) as GameObject;
			soulParticle.GetComponent<Sheep_Summon_Particle>().removeFunction = new PlayerSheepAI.callSheep(RemoveSheepFromList);
			soulParticle.GetComponent<Sheep_Summon_Particle>()?.InitSheepParticle(GetCurrentSheepPrefab(theSheepType), summonParticleLerpSpeed, hit.position, this, theSheepType);
            var module = soulParticle.GetComponent<ParticleSystem>().main;
            module.startColor = sheepFlocks[(int)theSheepType].UIColor;
            spawnParticles.Add(soulParticle);

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

    #region Sheep Attack
    public void OnSheepAttack(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            hasCharged = false;
        }

        //CHARGE
		/*
        if(context.started && isPreparingCharge && sheepFlocks[(int)SheepTypes.RAM].MaxSize > 0)
        {
			// shortened for brevity
			DoStampede();

			// dont bother looking at the next if
			return;
        }
		*/
        if (canAttack && !isPreparingCharge && !hasCharged)
        {
            SheepTypes flockType = currentFlockType;

			if (context.canceled)//(context.ReadValue<float>() == 0)
			{
				DoAttack();
            }
            else if (context.started && sheepAttackPoint == null)//context.ReadValue<float>() == 1)
			{
                //spawn icon
                var attackPoint = Instantiate(sheepAttackPointPrefab, transform.position, Quaternion.identity) as GameObject;
                ParticleSystem[] particleSystems = attackPoint.GetComponentsInChildren<ParticleSystem>();
                foreach(ParticleSystem particle in particleSystems)
                {
                    var module = particle.main;
                    module.startColor = sheepFlocks[(int)flockType].UIColor;
                }

                sheepAttackPoint = attackPoint;

                //prepare to attac
                isPreparingAttack = true;
            }

            
        }

    }
	void DoAttack()
	{
		SheepTypes flockType = currentFlockType;

		//stop ATTACK
		isPreparingAttack = false;

		//play animation
		animator.Play(attackAnimation);

        //TEMP SOUND
        FMODUnity.RuntimeManager.PlayOneShotAttached(abilitySound, gameObject);

		//get rid of icon
		Destroy(sheepAttackPoint);

		// making sure its gone for input checking
		sheepAttackPoint = null;

        //send sheep to point if valid!
        RaycastHit hit;
		if (Physics.Raycast(Camera.main.transform.position + attackPointOffset, Camera.main.transform.forward, out hit, Mathf.Infinity, attackTargetLayers))
		{
			//instantiate confirm prefab
			var attackConfirm = Instantiate(sheepAttackConfirmPrefab, hit.point, Quaternion.identity);
			ParticleSystem[] particleSystems = attackConfirm.GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem particle in particleSystems)
			{
				var module = particle.main;
				module.startColor = sheepFlocks[(int)flockType].UIColor;
			}

			for (int i = 0; i < GetSheepFlock(flockType).Count; i++)
			{
				if (GetSheepFlock(flockType)[i].IsCommandable())
					GetSheepFlock(flockType)[i]?.CreateListOfAttackTargets(hit.point, attackRadius);
			}
		}
		//start cooldown
		canAttack = false;

		//no sheep?
		if (SwapUIAnimator.gameObject.activeSelf && sheepFlocks[currentFlockIndex].activeSheep.Count <= 0)
			SwapUIAnimator.Play(noSheepAnimUI);

		StartCoroutine(AttackCooldown());
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

    #region Sheep Stampede
    public void OnSheepCharge(InputAction.CallbackContext context)
    {
		if (context.started && canCharge && !isPreparingAttack && !isPreparingCharge && currentFlockType == SheepTypes.RAM)
		{
			int flockType = (int)SheepTypes.RAM;

			//spawn icon
			var chargePoint = Instantiate(sheepChargePointPrefab, transform.position, Quaternion.identity) as GameObject;
			sheepChargePoint = chargePoint;

			//prepare to charge
			isPreparingCharge = true;

			//get the sheep to move to the player
			//recall current flock!
			for (int i = 0; i < sheepFlocks[flockType].activeSheep.Count; i++)
			{
				sheepFlocks[flockType].activeSheep[i]?.RecallSheep();
			}
		}
		else if (context.canceled && isPreparingCharge && sheepFlocks[(int)SheepTypes.RAM].MaxSize > 0)
		{
			DoStampede();
		}
	}
	void DoStampede()
	{
		SheepTypes flockType = SheepTypes.RAM;

		//stop charging
		isPreparingCharge = false;

		//has charged
		hasCharged = true;

		//play animation
		animator.Play(chargeAnimation);

		//TEMP SOUND
		FMODUnity.RuntimeManager.PlayOneShotAttached(abilitySound, gameObject);

		//get rid of icon
		Destroy(sheepChargePoint);

		sheepFlocks[(int)SheepTypes.RAM].spellParticle.Play(true);


		//send sheep to point if valid!
		RaycastHit hit;
		if (Physics.Raycast(Camera.main.transform.position + chargePointOffset, Camera.main.transform.forward, out hit, Mathf.Infinity, chargeTargetLayers))
		{
			for (int i = 0; i < GetSheepFlock(flockType).Count; i++)
			{
				if (GetSheepFlock(flockType)[i].IsCommandable() &&
					Vector3.Distance(transform.position, GetSheepFlock(flockType)[i].transform.position) <= chargeDistanceToUse)
					GetSheepFlock(flockType)[i]?.BeginCharge((hit.point- transform.position).normalized);
			}

			Instantiate(sheepChargeConfirmPrefab, hit.point, GetComponent<PlayerMovement>().playerOrientation.transform.rotation);
		}

		//if (sheepFlocks[(int)SheepTypes.RAM].currentSize <= 0) SwapUIAnimator.Play(noSheepAnimUI);

		//start cooldown
		canCharge = false;
		chargeIcon.CooldownUIEffect(chargeCooldown);
		StartCoroutine(ChargeCooldown());
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
                sheepChargePoint.transform.rotation = GetComponent<PlayerMovement>().playerOrientation.transform.rotation;
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
		if (isDefending)
		{

            //increase speed over time
            defendPivotRotateSpeed += defendPivotRotateSpeedGain * Time.deltaTime;
			defendPointPivot.Rotate(0f, defendPivotRotateSpeed * Time.deltaTime, 0f);

            //add to timer
            currentDefendTime += Time.deltaTime;

            if(currentDefendTime < defendTimeMax)
            {
                // check if you have any defending sheep
                for (int i = sheepFlocks[(int)SheepTypes.FLUFFY].activeSheep.Count - 1; i >= 0; i--)
                    if (sheepFlocks[(int)SheepTypes.FLUFFY].activeSheep[i].GetSheepState() == SheepStates.VORTEX)
                        return;
            }

			//vfx
			for (int i = 0; i < defendPoints.Count; i++)
			{
				defendPoints[i].gameObject.SetActive(false);
			}
			isDefending = false;
            EndSheepDefend();
        }
    }

    void EndSheepDefend()
    {
        Debug.Log("end defend");

        isDefending = false;

        // switching to be only useable by fluffy sheep, keep same architecture in case we change our minds (we probably won't)
        int flockType = (int)SheepTypes.FLUFFY;

        //shake camera
        playerCam.ShakeCamera(true);

        animator.Play(defendAnimation);

        //TEMP SOUND
        FMODUnity.RuntimeManager.PlayOneShotAttached(vortexEndSound, gameObject);
		vortexStartEmitter.Stop();

        sheepFlocks[(int)SheepTypes.FLUFFY].spellParticle.Play(true);

        for (int i = sheepFlocks[flockType].activeSheep.Count - 1; i >= 0; i--)
        {
            if (sheepFlocks[flockType].activeSheep[i].GetSheepState() == SheepStates.VORTEX)
            {
                sheepFlocks[flockType].activeSheep[i]?.EndDefendPlayer(sheepFlocks[(int)SheepTypes.FLUFFY].SheepProjectilePrefab);

                // delete all active sheep
                sheepFlocks[flockType].activeSheep[i].KillSheep();
            }
        }
        //sheepFlocks[flockType].activeSheep.Clear(); // i dont think that should be there? else it'll clear non-defending fluffys?


        //vfx
        for (int i = 0; i < defendPoints.Count; i++)
        {
            defendPoints[i].gameObject.SetActive(false);
        }

        //start cooldown
        canDefend = false;
        defendIcon.CooldownUIEffect(defendCooldown);
        StartCoroutine(DefendCooldown());
    }
    public void OnSheepDefend(InputAction.CallbackContext context)
    {
		if (canDefend && context.started)
		{
			if (!isDefending && currentFlockType == SheepTypes.FLUFFY)
			{
				Debug.Log("start defend");

				isDefending = true;

                sheepFlocks[(int)SheepTypes.FLUFFY].spellParticle.Play(true);

                //reset speed
                defendPivotRotateSpeed = defendRotateBaseSpeed;

				//switching to be only useable by fluffy sheep, keep same architecture in case we change our minds (we probably won't)
				SheepTypes flockType = SheepTypes.FLUFFY;


				animator.Play(defendAnimation);

				// SOUND
				vortexStartEmitter.Play();

				// get all following fluffys to follow player
				for (int i = 0; i < GetSheepFlock(flockType).Count; i++)
                {
					if (!GetSheepFlock(flockType)[i].IsCommandable())
                    {

                    }
					GetSheepFlock(flockType)[i]?.BeginDefendPlayer(defendPointPivot);
				}
					

				// turn on vfx
				for (int i = 0; i < defendPoints.Count; i++)
				{
					defendPoints[i].gameObject.SetActive(true);
				}

                //start timer
                currentDefendTime = 0;
			}
			else if (isDefending)
			{
                EndSheepDefend();
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
            isLaunching = true;
        else if(context.canceled)
            isLaunching = false;
    }
	void CheckLaunch()
	{
		if (isLaunching && canLaunch)
		{
			/*
			PlayerSheepAI sheepToLaunch = null;
			float currentLaunchDistance = 999f;
			
			
			for(int j = 0; j < 3; j++)
			{
			    for(int i = 0; i < GetSheepFlock((SheepTypes)j).Count; i++)
			    {
			        if (Vector3.Distance(transform.position, GetSheepFlock((SheepTypes)j)[i].transform.position) < minDistanceToLaunch
			                && currentLaunchDistance > Vector3.Distance(transform.position, GetSheepFlock((SheepTypes)j)[i].transform.position))
			        {
			            currentLaunchDistance = Vector3.Distance(transform.position, GetSheepFlock((SheepTypes)j)[i].transform.position);
			            sheepToLaunch = GetSheepFlock((SheepTypes)j)[i];
			
			            flockIndex = j;
			            sheepIndex = i;
			        }
			    }
			}
			
			if(sheepToLaunch != null)
			{
			    animator.Play(launchAnimation);
			
			    PlayerSheepProjectile launchSheep =
			        Instantiate(sheepFlocks[flockIndex].SheepProjectilePrefab, launchOrigin.position, launchOrigin.rotation)
			        .GetComponent<PlayerSheepProjectile>();
			
			    launchSheep.isBlackSheep = GetSheepFlock((SheepTypes)flockIndex)[sheepIndex].isBlackSheep;
			    launchSheep.LaunchProjectile();
			
			    GetSheepFlock((SheepTypes)flockIndex)[sheepIndex].KillSheep();
			}
			*/

			//OLD IMPLEMENTATION KEEP IN CASE WE GO BACK
			SheepTypes flockType = currentFlockType;

			//do we have any sheep? 
			if (GetSheepFlock(flockType).Count > 0)
			{
				//are sheep nearby?
				for (int i = 0; i < GetSheepFlock(flockType).Count; i++)
				{
					if (Vector3.Distance(transform.position, GetSheepFlock(flockType)[i].transform.position) <= minDistanceToLaunch)
					{
						// rotate Bo to look in direction of launch
						transform.eulerAngles = new Vector3(0, launchOrigin.rotation.eulerAngles.y, 0);
						animator.Rebind(); // Rebind resets the animation state, allowing up to restart launch if its in progress
						animator.Play(launchAnimation);

						// launch that mf
						PlayerSheepProjectile launchSheep =
							Instantiate(sheepFlocks[currentFlockIndex].SheepProjectilePrefab, launchOrigin.position, launchOrigin.rotation)
							.GetComponent<PlayerSheepProjectile>();

						launchSheep.isBlackSheep = GetSheepFlock(flockType)[i].isBlackSheep;
						launchSheep.LaunchProjectile();

						GetSheepFlock(flockType)[i].KillSheep();

						// break loop
						break;
					}
				}
			}
            else SwapUIAnimator.Play(noSheepAnimUI);

            //start cooldown
            canLaunch = false;
			//launchIcon.CooldownUIEffect(launchCooldown);
			StartCoroutine(SheepLaunchCooldown());
		}
	}

    IEnumerator SheepLaunchCooldown()
    {
        yield return new WaitForSeconds(launchCooldown);
        canLaunch = true;
    }
	#endregion

	// quick modulo utility function
	int Mod(int a, int n) => (a % n + n) % n;
}
