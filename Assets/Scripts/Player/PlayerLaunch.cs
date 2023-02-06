using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLaunch : MonoBehaviour
{
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

	// components
	PlayerSheepAbilities flocks;
	Animator animator;

	// Start is called before the first frame update
	void Start()
    {
		flocks = GetComponent<PlayerSheepAbilities>();
		animator = GetComponent<PlayerAnimationController>().playerAnimator;
	}

	// Update is called once per frame
	void Update()
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
			SheepTypes flockType = flocks.currentFlockType;

			//do we have any sheep? 
			if (flocks.GetActiveSheep(flockType).Count > 0)
			{
				//are sheep nearby?
				for (int i = 0; i < flocks.GetActiveSheep(flockType).Count; i++)
				{
					if (Vector3.Distance(transform.position, flocks.GetActiveSheep(flockType)[i].transform.position) <= minDistanceToLaunch)
					{
						// rotate Bo to look in direction of launch
						transform.eulerAngles = new Vector3(0, launchOrigin.rotation.eulerAngles.y, 0);
						animator.Rebind(); // Rebind resets the animation state, allowing up to restart launch if its in progress
						animator.Play(launchAnimation);

						// launch that mf
						PlayerSheepProjectile launchSheep =
							Instantiate(flocks.GetSheepFlock(flockType).SheepProjectilePrefab, launchOrigin.position, launchOrigin.rotation)
							.GetComponent<PlayerSheepProjectile>();

						launchSheep.isBlackSheep = flocks.GetActiveSheep(flockType)[i].isBlackSheep;
						launchSheep.LaunchProjectile();

						flocks.GetActiveSheep(flockType)[i].KillSheep();

						// break loop
						break;
					}
				}
			}
			else WorldState.instance.HUD.SheepErrorAnimation();

			//start cooldown
			canLaunch = false;
			//launchIcon.CooldownUIEffect(launchCooldown);
			StartCoroutine(SheepLaunchCooldown());
		}
	}


	#region Sheep Launch
	public void OnSheepLaunch(InputAction.CallbackContext context)
	{
		if (context.started)
			isLaunching = true;
		else if (context.canceled)
			isLaunching = false;
	}

	IEnumerator SheepLaunchCooldown()
	{
		yield return new WaitForSeconds(launchCooldown);
		canLaunch = true;
	}
	#endregion
}
