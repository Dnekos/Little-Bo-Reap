using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBuildWall : MonoBehaviour
{
	[Header("Charge Point")]
	[SerializeField] Vector3 raycastOffset;
	[SerializeField] Vector3 prefabSpawnOffset;
	[SerializeField] GameObject wallPrefab;
	[SerializeField] GameObject confirmPrefab;
	[SerializeField] LayerMask targetLayers;

	[Header("Bo Peep")]
	[SerializeField] string confirmAnimation;
	[SerializeField] AbilityIcon chargeIcon;
	[SerializeField] float chargeCooldown = 1f;

	[Header("Preparing")]
	[SerializeField] float distanceToUse = 20;
	[SerializeField] float recallingStopDist = 5;

	bool canCharge = true;
	GameObject sheepChargePoint;
	Vector3 chargePosition;
	[HideInInspector] public bool isPreparing, hasCharged;

	[Header("Sound")]
	[SerializeField] FMODUnity.EventReference abilitySound;

	PlayerSheepAbilities flocks;
	Animator animator;
	PlayerAttackCommand attack;

	// Start is called before the first frame update
	void Start()
	{
		attack = GetComponent<PlayerAttackCommand>();
		flocks = GetComponent<PlayerSheepAbilities>();
		animator = GetComponent<PlayerAnimationController>().playerAnimator;
	}

	// Update is called once per frame
	void Update()
	{
		if (isPreparing)
		{
			//draw ray from camera forward to point
			RaycastHit hit;
			if (Physics.Raycast(Camera.main.transform.position + prefabSpawnOffset, Camera.main.transform.forward, out hit, Mathf.Infinity, targetLayers))
			{
				//draw charge point
				sheepChargePoint.transform.position = hit.point + prefabSpawnOffset;
				sheepChargePoint.transform.rotation = GetComponent<PlayerMovement>().playerOrientation.transform.rotation;

			}
			else
			{
				//draw it way the fuck down so it isnt seen
				sheepChargePoint.transform.position = new Vector3(0f, -1000f, 0f);
			}

		}
	}


	#region Sheep Stampede
	public void OnSheepWall(InputAction.CallbackContext context)
	{
		SheepTypes flockType = flocks.currentFlockType;
		List<PlayerSheepAI> sheep = flocks.GetActiveSheep(flockType);

		if (context.started && canCharge && !attack.isPreparingAttack && !isPreparing
			&& sheep.Count > 0
			&& !(sheep[0].ability is SheepStampedeBehavior || sheep[0].ability is SheepVortexBehavior))
		{
			// destroy old wall
			if (sheepChargePoint != null && sheepChargePoint.activeInHierarchy)
			{
				Debug.Log("whoops");
				Destroy(sheepChargePoint);
			}
			// spawn wall
			var chargePoint = Instantiate(wallPrefab, transform.position, Quaternion.identity) as GameObject;
			sheepChargePoint = chargePoint;

			//prepare to charge
			isPreparing = true;

			//get the sheep to move to the player
			//recall current flock!
			for (int i = 0; i < sheep.Count; i++)
			{
				sheep[i]?.RecallSheep();
				//sheep[i].SetStopDist(recallingStopDist);
			}
		}
		else if (context.canceled && isPreparing && flocks.GetSheepFlock(flockType).MaxSize > 0)
		{
			if (sheepChargePoint.transform.position == new Vector3(0f, -1000f, 0f))
			{
				Destroy(sheepChargePoint);
				StartCooldown();
			}
			else
				DoStampede();
		}
	}
	void DoStampede()
	{
		SheepTypes flockType = SheepTypes.RAM;
		
		StartCooldown();

		//play animation
		animator.Play(confirmAnimation);

		//TEMP SOUND
		FMODUnity.RuntimeManager.PlayOneShotAttached(abilitySound, gameObject);

		//get rid of icon
		//Destroy(sheepChargePoint);
		sheepChargePoint.GetComponent<SheepConstruct>().Interact();

		flocks.GetSheepFlock(SheepTypes.RAM).spellParticle.Play(true);

		//send sheep to point if valid!
		RaycastHit hit;
		if (Physics.Raycast(Camera.main.transform.position + prefabSpawnOffset, Camera.main.transform.forward, out hit, Mathf.Infinity, targetLayers))
		{
			for (int i = 0; i < flocks.GetActiveSheep(flockType).Count; i++)
			{
				if (flocks.GetActiveSheep(flockType)[i].IsCommandable() &&
					Vector3.Distance(transform.position, flocks.GetActiveSheep(flockType)[i].transform.position) <= distanceToUse)
					flocks.GetActiveSheep(flockType)[i]?.BeginAbility((hit.point - transform.position).normalized);
			}

			Instantiate(confirmPrefab, hit.point, GetComponent<PlayerMovement>().playerOrientation.transform.rotation);
		}

		//if (sheepFlocks[(int)SheepTypes.RAM].currentSize <= 0) SwapUIAnimator.Play(noSheepAnimUI);
	}

	void StartCooldown()
	{
		//stop charging
		isPreparing = false;

		//has charged
		hasCharged = true;
		canCharge = false;
		chargeIcon.CooldownUIEffect(chargeCooldown);
		StartCoroutine(ChargeCooldown());
	}

	IEnumerator ChargeCooldown()
	{
		yield return new WaitForSeconds(chargeCooldown);
		canCharge = true;
	}
	#endregion

}
