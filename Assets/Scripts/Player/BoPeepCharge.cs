//REVIEW: Would like a comment on why we have to change the players max move speed for this charge attack
		//	(I know why you have to, but someone who isn't in the code base regularly might scratch their heads looking at that)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class BoPeepCharge : MonoBehaviour
{
	[Header("Stampeding")]
	[SerializeField] float chargeSpeed = 35f;
	//[SerializeField] float chargePointRadius = 10f;
	//[SerializeField] float chargeStopDistance = 0f;
	//[SerializeField] float destinationDist = 20f;

	[Header("End Condition")]
	[SerializeField] float chargeDuration = 1.5f;

	[SerializeField] PlayerMovement moveController;
	bool charging;//REVIEW: mentioned this on another review, but I like 'isCharging' more as a name for bools(I'm nitpicking)

	//hold reg max move speed to set it back after.
	float maxMoveSpeedTemp;

	private void Update()
	{
		if(charging)
		{
			AbilityUpdate();
		}
	}

	public void AbilityUpdate()
	{
		Vector2 MovementVector = new Vector2(0, 1);//REVIEW: maybe make this Vector2 a variable that can be modified in engine
															//Or, Im guessing you just want to move forward, so maybe you just want to use the player's forward vector
		moveController.SetMovementVector(MovementVector);
		//DEPRECATED
		/*
		//set destination
		//get random point inside radius
		Vector3 chargePosition = this.transform.position + this.transform.forward * destinationDist;

		//if inside navmesh, charge!
		if (NavMesh.SamplePosition(chargePosition, out NavMeshHit hit, chargePointRadius, 1))
		{
			//get charge
			chargePosition = hit.position;

			//set agent destination
			agent.destination = chargePosition;
			Debug.Log("sick charge spot can I try");
		}
		else
		{
			agent.destination = chargePosition;
			Debug.Log("we didnt find nothin");
			// end charge
			End();
		}
		*/
	}

	public void Begin(Vector3 targettedPos)
	{
		Debug.Log("triggering bo peep charge");
		charging = true;
		//set timer to 0
		this.StartCoroutine(ChargeTimer());

		Vector2 MovementVector = new Vector2(0, 1);//REVIEW: same as above
  		maxMoveSpeedTemp = GetComponent<PlayerMovement>().GetMaxMoveSpeed();
		moveController.SetMaxMoveSpeed(chargeSpeed);
		moveController.SetMovementVector(MovementVector);
		GetComponent<PlayerInput>().SwitchCurrentActionMap("RamCharge");
		/*
		rb.isKinematic = true;
		agent.enabled = true;

		//set destination
		//targettedPos.y = this.transform.position.y;
		this.transform.LookAt(targettedPos + this.transform.position);
		AbilityUpdate();

		//set charge speed
		agent.speed = chargeSpeed;

		agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;		
		*/
	}

	IEnumerator ChargeTimer()
	{
		yield return new WaitForSeconds(chargeDuration);
		if (charging)
			End();
	}

	void End()
	{
		GetComponent<PlayerInput>().SwitchCurrentActionMap("PlayerMovement");
		moveController.SetMovementVector(Vector2.zero);
		moveController.SetMaxMoveSpeed(maxMoveSpeedTemp);
		charging = false;
		//agent.enabled = false;
		//rb.isKinematic = false;
	}
	public void OnStopCharge(InputAction.CallbackContext context)
	{
		End();
		moveController.OnJump(context);
	}
}
