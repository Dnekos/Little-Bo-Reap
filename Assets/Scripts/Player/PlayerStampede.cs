using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStampede : MonoBehaviour
{
	[Header("Charge Point")]
	[SerializeField] Vector3 chargePointOffset;
	[SerializeField] GameObject sheepChargePointPrefab;
	[SerializeField] GameObject sheepChargeConfirmPrefab;
	[SerializeField] LayerMask chargeTargetLayers;

	[Header("Bo Peep")]
	[SerializeField] string chargeAnimation;
	[SerializeField] AbilityIcon chargeIcon;
	[SerializeField] float chargeCooldown = 1f;

	[Header("Preparing")]
	[SerializeField] float distanceToUse = 20;
	[SerializeField] float recallingStopDist = 5;

	bool canCharge = true;
	GameObject sheepChargePoint;
	Vector3 chargePosition;
	[HideInInspector] public bool isPreparingCharge, hasCharged;

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
		if (isPreparingCharge)
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


	#region Sheep Stampede
	public void OnSheepCharge(InputAction.CallbackContext context)
	{
		SheepTypes flockType = flocks.currentFlockType;
		List<PlayerSheepAI> sheep = flocks.GetActiveSheep(flockType);

		if (context.started && canCharge && !attack.isPreparingAttack && !isPreparingCharge
			&& sheep.Count > 0
			&& sheep[0].ability is SheepStampedeBehavior)
		{
			//spawn icon
			var chargePoint = Instantiate(sheepChargePointPrefab, transform.position, Quaternion.identity) as GameObject;
			sheepChargePoint = chargePoint;

			//prepare to charge
			isPreparingCharge = true;

			//get the sheep to move to the player
			//recall current flock!
			for (int i = 0; i < sheep.Count; i++)
			{
				sheep[i]?.RecallSheep();
				//sheep[i].SetStopDist(recallingStopDist);
			}
		}
		else if (context.canceled && isPreparingCharge && flocks.GetSheepFlock(flockType).MaxSize > 0)
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

		flocks.GetSheepFlock(SheepTypes.RAM).spellParticle.Play(true);

		//send sheep to point if valid!
		RaycastHit hit;
		if (Physics.Raycast(Camera.main.transform.position + chargePointOffset, Camera.main.transform.forward, out hit, Mathf.Infinity, chargeTargetLayers))
		{
			for (int i = 0; i < flocks.GetActiveSheep(flockType).Count; i++)
			{
				if (flocks.GetActiveSheep(flockType)[i].IsCommandable() &&
					Vector3.Distance(transform.position, flocks.GetActiveSheep(flockType)[i].transform.position) <= distanceToUse)
					flocks.GetActiveSheep(flockType)[i]?.BeginAbility((hit.point - transform.position).normalized);
			}

			Instantiate(sheepChargeConfirmPrefab, hit.point, GetComponent<PlayerMovement>().playerOrientation.transform.rotation);
		}

		//if (sheepFlocks[(int)SheepTypes.RAM].currentSize <= 0) SwapUIAnimator.Play(noSheepAnimUI);

		//start cooldown
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
