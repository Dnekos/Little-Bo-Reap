using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttackCommand : MonoBehaviour
{
	[Header("Sheep Attack Variables")]
	[SerializeField] Vector3 attackPointOffset;
	[SerializeField] LayerMask attackTargetLayers;
	[SerializeField] string attackAnimation;
	[SerializeField] AbilityIcon attackIcon;
	[SerializeField] float attackCooldown = 1f;
	[SerializeField] float attackRadius = 10f;
	bool canAttack = true;
	GameObject sheepAttackPoint;
	Vector3 attackPosition;
	[HideInInspector] public bool isPreparingAttack;

	[Header("Sounds")]
	[SerializeField] FMODUnity.EventReference abilitySound;

	// components
	PlayerSheepAbilities flocks;
	Animator animator;
	PlayerStampede stampede;

	// Start is called before the first frame update
	void Start()
    {
		flocks = GetComponent<PlayerSheepAbilities>();
		animator = GetComponent<PlayerAnimationController>().playerAnimator;
		stampede = GetComponent<PlayerStampede>();
	}

	// Update is called once per frame
	void Update()
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

	#region Sheep Attack
	public void OnSheepAttack(InputAction.CallbackContext context)
	{			
		SheepTypes flockType = flocks.currentFlockType;
		//if (flocks.GetActiveSheep(flockType).Count <= 0)
		//	return;

		if (context.started)
		{
			stampede.hasCharged = false;
		
			// no sheep?
			if (flocks.GetActiveSheep(flockType).Count <= 0)
			{
				WorldState.instance.HUD.SheepErrorAnimation();
				return;
			}

        }

        if (canAttack && !stampede.isPreparingCharge && !stampede.hasCharged)
		{

			if (context.canceled && flocks.GetActiveSheep(flockType).Count > 0)
			{
				DoAttack();
			}
			else if (context.started && sheepAttackPoint == null)
			{
				//spawn icon
				var attackPoint = Instantiate(flocks.GetSheepFlock(flockType).reticleSustainPrefab, transform.position, Quaternion.identity) as GameObject;
				ParticleSystem[] particleSystems = attackPoint.GetComponentsInChildren<ParticleSystem>();
				foreach (ParticleSystem particle in particleSystems)
				{
					var module = particle.main;
					module.startColor = flocks.GetSheepFlock(flockType).UIColor;
				}

				sheepAttackPoint = attackPoint;

				//prepare to attac
				isPreparingAttack = true;
			}


		}

	}
	void DoAttack()
	{
		SheepTypes flockType = flocks.currentFlockType;

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
			var attackConfirm = Instantiate(flocks.GetSheepFlock(flockType).reticleConfirmPrefab, hit.point, Quaternion.identity);
			ParticleSystem[] particleSystems = attackConfirm.GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem particle in particleSystems)
			{
				var module = particle.main;
				module.startColor = flocks.GetSheepFlock(flockType).UIColor;
			}

			for (int i = 0; i < flocks.GetActiveSheep(flockType).Count; i++)
			{
				if (flocks.GetActiveSheep(flockType)[i].IsCommandable())
					flocks.GetActiveSheep(flockType)[i]?.CreateListOfAttackTargets(hit.point, attackRadius);
			}
		}
		//start cooldown
		canAttack = false;

		StartCoroutine(AttackCooldown());
	}


	IEnumerator AttackCooldown()
	{
		yield return new WaitForSeconds(attackCooldown);
		canAttack = true;
	}
	#endregion
}
