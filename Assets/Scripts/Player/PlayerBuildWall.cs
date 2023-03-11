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
	[SerializeField] GameObject bellPrefab;
	[SerializeField] GameObject confirmPrefab;
	[SerializeField] LayerMask targetLayers;
	[SerializeField, Tooltip("likely depricated, rotates raycast around x axix")] float xAngleOffset = 0;
	[SerializeField, Tooltip("how steep the wall can be built")] float maxSlope = 30;

	[Header("Bo Peep")]
	[SerializeField] string confirmAnimation;
	[SerializeField] AbilityIcon chargeIcon;
	[SerializeField] float cooldown = 1f;

	// Preparing
	bool canCharge = true;
	GameObject sheepChargePoint;
	Vector3 chargePosition;
	[HideInInspector] public bool isPreparing;

	[Header("Sound")]
	[SerializeField] FMODUnity.EventReference abilitySound;

	PlayerSheepAbilities flocks;
	Animator animator;
	PlayerAttackCommand attack;
	PlayerMovement player;

	// Start is called before the first frame update
	void Start()
	{
		attack = GetComponent<PlayerAttackCommand>();
		flocks = GetComponent<PlayerSheepAbilities>();
		animator = GetComponent<PlayerAnimationController>().playerAnimator;
		player = GetComponent<PlayerMovement>();
	}

	// Update is called once per frame
	void Update()
	{
		if (isPreparing)
		{
			//draw ray from camera forward to point
			RaycastHit hit;
			if (Physics.Raycast(Camera.main.transform.position + raycastOffset, Quaternion.AngleAxis(xAngleOffset,Camera.main.transform.right)  * Camera.main.transform.forward , out hit, Mathf.Infinity, targetLayers) 
				&& Vector3.Angle(hit.normal,Vector3.up) < maxSlope)
			{
				//draw charge point
				sheepChargePoint.transform.position = hit.point + prefabSpawnOffset;
				sheepChargePoint.transform.rotation = player.playerOrientation.transform.rotation;

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

		// no sheep?
		if (flocks.GetActiveSheep(flockType).Count <= 0)
		{
			WorldState.instance.HUD.SheepErrorAnimation();
			return;
		}


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
			GameObject constructPrefab;
			// spawn wall
			if(WorldState.instance.passiveValues.activeBuilderAbility == "Ability 1")
			{
				constructPrefab = bellPrefab;
			}
			else
			{
				constructPrefab = wallPrefab;
			}

			var chargePoint = Instantiate(constructPrefab, transform.position, Quaternion.identity) as GameObject;
			sheepChargePoint = chargePoint;

			//prepare to charge
			isPreparing = true;

			//get the sheep to move to the player
			//recall current flock!
			// TODO: move sheep to wall
			for (int i = 0; i < sheep.Count; i++)
			{
				sheep[i]?.RecallSheep();
			}
		}
		else if (context.canceled && isPreparing && flocks.GetSheepFlock(flockType).MaxSize > 0)
		{
			if (sheepChargePoint.transform.position == new Vector3(0f, -1000f, 0f))
			{
				Destroy(sheepChargePoint);
				isPreparing = false;

				//StartCooldown(); // seems too much to do cooldown on a bad input
			}
			else
				DoWall();
		}
	}
	void DoWall()
	{
		StartCooldown();

		//play animation
		animator.Play(confirmAnimation);

		//TEMP SOUND
		FMODUnity.RuntimeManager.PlayOneShotAttached(abilitySound, gameObject);

		// Set up sheep
		sheepChargePoint.GetComponent<Interactable>().Interact();

		// confirm juice
		flocks.GetSheepFlock(SheepTypes.BUILD).spellParticle.Play(true);
		Instantiate(confirmPrefab, sheepChargePoint.transform.position - prefabSpawnOffset, player.playerOrientation.transform.rotation);
	}

	void StartCooldown()
	{
		//stop charging
		isPreparing = false;

		//has charged
		canCharge = false;
		chargeIcon.CooldownUIEffect(cooldown);
		StartCoroutine(AbilityCooldown());
	}

	IEnumerator AbilityCooldown()
	{
		yield return new WaitForSeconds(cooldown);
		canCharge = true;
	}
	#endregion

}
