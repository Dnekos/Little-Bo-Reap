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
	public GameObject reticleSustainPrefab;
	public GameObject reticleConfirmPrefab;
}

public class PlayerSheepAbilities : MonoBehaviour
{

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
	[SerializeField] List<ParticleSystem> bellParticles;
	[SerializeField] ParticleSystem bellParticleBurst;
    [SerializeField] ReticleControls reticleControls;
	[SerializeField] List<ParticleSystem> failSummonParticles;

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
	[SerializeField] List<GameObject> summonParticle;
	[SerializeField] float summonParticleLerpSpeed = 5f;
	[SerializeField] GameEvent endConstructsEvent;
	List<GameObject> spawnParticles;
	bool canSummonSheep = true;
	//bool canSummonAllSheep = true;
	PlayerSummoningResource summonResource;
	bool summonPerformed = false;
	bool recallPerformed = false;

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

		StartCoroutine(Initialize()); // slow but prevents this being called before HUD is set up
	}

	/// <summary>
	/// needed for set up that may have blockers (other things in start)
	/// </summary>
	private IEnumerator Initialize()
	{
		yield return new WaitForEndOfFrame();
		// get flock totals from save data
		SaveData sd = WorldState.instance.PersistentData;
		if (sd.currentCheckpoint > -1) // only set sheep if we are past the first checkpoint, so as to not overwrite starting sheep
		{
			sheepFlocks[0].MaxSize = sd.totalBuilder;
			sheepFlocks[1].MaxSize = sd.totalRam;
			sheepFlocks[2].MaxSize = sd.totalFluffy;
		}
		UpdateFlockUI();
	}

	private void Update()
	{
		// set leader
		for (int i = 0; i < 3; i++)
			for (int j = 0; j < sheepFlocks[i].activeSheep.Count; j++)
				sheepFlocks[i].activeSheep[j].leaderSheep = sheepFlocks[i].activeSheep[0];
	}

	#region Utility
	public void ShakeCamera()
	{
		playerCam.ShakeCamera(true);
	}
	public int GetTotalSheepCount()
	{
		return sheepFlocks[0].activeSheep.Count + sheepFlocks[1].activeSheep.Count + sheepFlocks[2].activeSheep.Count;
	}

	#endregion

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
			//var particleModule = bellParticles.main;
			//particleModule.startColor = sheepFlocks[currentFlockIndex].UIColor;

			for(int i = 0; i < bellParticles.Count; i++)
            {
				if (currentFlockIndex == i) bellParticles[i].gameObject.SetActive(true);
				else bellParticles[i].gameObject.SetActive(false);         
            }

			sheepFlocks[currentFlockIndex].flockChangeParticle.Play(true);
			FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Sheep", currentFlockIndex);

			WorldState.instance.HUD.SwapAnimation();

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

			//var particleModule = bellParticles.main;
			//particleModule.startColor = sheepFlocks[currentFlockIndex].UIColor;

			for (int i = 0; i < bellParticles.Count; i++)
			{
				if (currentFlockIndex == i) bellParticles[i].gameObject.SetActive(true);
				else bellParticles[i].gameObject.SetActive(false);
			}

			sheepFlocks[currentFlockIndex].flockChangeParticle.Play(true);
			FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Sheep", currentFlockIndex);

			WorldState.instance.HUD.SwapAnimation();

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

			WorldState.instance.HUD.EnableSheepWheel();
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


			//var particleModule = bellParticles.main;
			//particleModule.startColor = sheepFlocks[currentFlockIndex].UIColor;
			sheepFlocks[currentFlockIndex].flockChangeParticle.Play(true);

			WorldState.instance.HUD.SwapAnimation();

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

				//var particleModule = bellParticles.main;
				//particleModule.startColor = sheepFlocks[currentFlockIndex].UIColor;
				//sheepFlocks[currentFlockIndex].flockChangeParticle.Play(true);

				WorldState.instance.HUD.SwapAnimation();

				UpdateFlockUI();
			}

			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;

			//disable flock select menu
			isInFlockMenu = false;
			WorldState.instance.HUD.DisableSheepWheel();
		}
	}
	#endregion
	public List<PlayerSheepAI> GetActiveSheep(SheepTypes theFlockType)
	{
		return sheepFlocks[(int)theFlockType].activeSheep;
	}
	public List<PlayerSheepAI> GetActiveSheep(int theFlockIndex)
	{
		return sheepFlocks[theFlockIndex].activeSheep;
	}
	public Flock GetSheepFlock(SheepTypes theFlockType)
	{
		return sheepFlocks[(int)theFlockType];
	}
	public Flock GetSheepFlock(int theFlockIndex)
	{
		return sheepFlocks[theFlockIndex];
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
		HUDManager hud = WorldState.instance.HUD;
		for (int i = 0; i < sheepFlocks.Length; i++)
			hud.UpdateFlockWheelText(i, sheepFlocks[i].activeSheep.Count, sheepFlocks[i].MaxSize);

		hud.UpdateActiveFlockUI(currentFlockIndex,
			sheepFlocks[currentFlockIndex].activeSheep.Count + "/" + sheepFlocks[currentFlockIndex].MaxSize,
			sheepFlocks[currentFlockIndex].UIColor);

		reticleControls.SetReticule(currentFlockType);
	}

	bool CheckIfCloseToLeader(SheepTypes theSheepType)
	{
		// if you have a flock and you're close to a given leader, you can use an ability.
		return GetActiveSheep(theSheepType).Count > 0 &&
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
			for (int j = 0; j < GetActiveSheep((SheepTypes)i).Count; j++)
			{
				GetActiveSheep((SheepTypes)i)[j].GothMode();
				Instantiate(gothExplosion, GetActiveSheep((SheepTypes)i)[j].transform.position, Quaternion.identity);
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
		if (context.started && canSummonSheep && sheepFlocks[currentFlockIndex].MaxSize > 0 && summonResource.GetCurrentBlood() >= summonBloodCost)
		{
			summonResource.ChangeBloodAmount(-summonBloodCost);

			//disallow summoning
			canSummonSheep = false;

			//get flock type
			SheepTypes flockType = currentFlockType;

			//play animation
			animator.Play(summonAnimation);

			sheepFlocks[(int)flockType].spellParticle.Play(true);

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
			for (int i = GetActiveSheep(flockType).Count - 1; i >= 0; i--)
			{
				// ignore list slots that are null (dead sheep)
				if (GetActiveSheep(flockType)[i] != null)
				{
					GetActiveSheep(flockType)[i].GibSheep();
					GetActiveSheep(flockType)[i].KillSheep();
				}
			}
			GetActiveSheep(flockType).Clear();

			int amountToSummon = GetSheepAmountToSummon(flockType);

			//summon sheep!
			for (int i = 0; i < amountToSummon; i++)
			{
				StartCoroutine(SummonSheep(flockType, i));
			}

			while( WorldState.instance.SheepPool[currentFlockIndex].Count < amountToSummon)
			{
				WorldState.instance.SheepPool[currentFlockIndex].Add(null);
			}

			//start cooldown
			StartCoroutine(SummonSheepCooldown());

			//TEMP SOUND
			FMODUnity.RuntimeManager.PlayOneShotAttached(summonSound, gameObject);
			//Debug.Log("summon sound play");

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
		else if(context.started && canSummonSheep && sheepFlocks[currentFlockIndex].MaxSize > 0 && summonResource.GetCurrentBlood() <= summonBloodCost)
        {
			failSummonParticles[(int)currentFlockType].Play(true);
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

	IEnumerator SummonSheep(SheepTypes theSheepType, int index)
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
			var soulParticle = Instantiate(summonParticle[currentFlockIndex], transform.position, Quaternion.identity) as GameObject;
			soulParticle.GetComponent<Sheep_Summon_Particle>().removeFunction = new PlayerSheepAI.callSheep(RemoveSheepFromList);
			soulParticle.GetComponent<Sheep_Summon_Particle>()?.InitSheepParticle(GetCurrentSheepPrefab(theSheepType), summonParticleLerpSpeed, hit.position, this, theSheepType, index);
			//var module = soulParticle.GetComponent<ParticleSystem>().main;
			//module.startColor = sheepFlocks[(int)theSheepType].UIColor;
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


	// quick modulo utility function
	int Mod(int a, int n) => (a % n + n) % n;
}