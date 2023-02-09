using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerVortex : MonoBehaviour
{
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

	// components
	PlayerSheepAbilities flocks;
	Animator animator;

	// Start is called before the first frame update
	void Start()
    {
		flocks = GetComponent<PlayerSheepAbilities>();
		animator = GetComponent<PlayerAnimationController>().playerAnimator;

		//set defend distance of each defence point
		for (int i = 0; i < defendPoints.Count; i++)
		{
			defendPoints[i].position += defendPoints[i].forward * defendDistance;
		}

		defendRotateBaseSpeed = defendPivotRotateSpeed;
	}

	// Update is called once per frame
	void Update()
    {
		if (isDefending)
		{

			//increase speed over time
			defendPivotRotateSpeed += defendPivotRotateSpeedGain * Time.deltaTime;
			defendPointPivot.Rotate(0f, defendPivotRotateSpeed * Time.deltaTime, 0f);

			//add to timer
			currentDefendTime += Time.deltaTime;

			if (currentDefendTime < defendTimeMax)
			{
				// check if you have any defending sheep
				for (int i = flocks.GetActiveSheep(SheepTypes.FLUFFY).Count - 1; i >= 0; i--)
					if (flocks.GetActiveSheep(SheepTypes.FLUFFY)[i].GetSheepState() == SheepStates.VORTEX)
						return;
			}

			//vfx
			for (int i = 0; i < defendPoints.Count; i++)
			{
				defendPoints[i].gameObject.SetActive(false);
			}
			Debug.Log("ending?");
			isDefending = false;
			EndSheepDefend();
		}
	}

	void EndSheepDefend()
	{
		if (flocks.GetActiveSheep(SheepTypes.FLUFFY).Count <= 0)
			return;

		Debug.Log("end defend");
		isDefending = false;

		//shake camera
		flocks.ShakeCamera();

		animator.Play(defendAnimation);

		//TEMP SOUND
		FMODUnity.RuntimeManager.PlayOneShotAttached(vortexEndSound, gameObject);
		vortexStartEmitter.Stop();

		flocks.GetSheepFlock(SheepTypes.FLUFFY).spellParticle.Play(true);

		for (int i = flocks.GetActiveSheep(SheepTypes.FLUFFY).Count - 1; i >= 0; i--)
		{
			if (flocks.GetActiveSheep(SheepTypes.FLUFFY)[i].GetSheepState() == SheepStates.VORTEX)
			{
				flocks.GetActiveSheep(SheepTypes.FLUFFY)[i]?.EndDefendPlayer(flocks.GetSheepFlock(SheepTypes.FLUFFY).SheepProjectilePrefab);

				// delete all active sheep
				flocks.GetActiveSheep(SheepTypes.FLUFFY)[i].KillSheep();
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
			if (!isDefending && flocks.currentFlockType == SheepTypes.FLUFFY)
			{
				Debug.Log("start defend");

				isDefending = true;

				flocks.GetSheepFlock(SheepTypes.FLUFFY).spellParticle.Play(true);

				//reset speed
				defendPivotRotateSpeed = defendRotateBaseSpeed;

				//switching to be only useable by fluffy sheep, keep same architecture in case we change our minds (we probably won't)
				SheepTypes flockType = SheepTypes.FLUFFY;


				animator.Play(defendAnimation);

				// SOUND
				vortexStartEmitter.Play();

				// get all following fluffys to follow player
				for (int i = 0; i < flocks.GetActiveSheep(SheepTypes.FLUFFY).Count; i++)
				{
					if (flocks.GetActiveSheep(SheepTypes.FLUFFY)[i].IsCommandable())
					{
						flocks.GetActiveSheep(SheepTypes.FLUFFY)[i]?.BeginDefendPlayer(defendPointPivot);
					}
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
		yield return new WaitForSeconds(defendCooldown);
		canDefend = true;
	}

}