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
	[SerializeField] Attack ramAttack;
	//[SerializeField] float chargePointRadius = 10f;
	//[SerializeField] float chargeStopDistance = 0f;
	//[SerializeField] float destinationDist = 20f;

	[Header("End Condition")]
	[SerializeField] float chargeDuration = 1.5f;
	[SerializeField] PlayerMovement moveController;

	[Header("Stop Killing Yourself")]
	[SerializeField] LayerMask groundLayer;
	[SerializeField] float forwardCliffOffset = 5;
	[SerializeField] float forwardCliffCheckDistance = 3;
	[SerializeField] float wallCheckDistance = 2;
	[SerializeField] float wallCheckOffset = 1.3f;
	
	bool isGoingToDie;
	bool isCharging;

	//hold reg max move speed to set it back after.
	float maxMoveSpeedTemp;

	private void Update()
	{
		if(isCharging)
		{
			AbilityUpdate();
		}
	}

	public void AbilityUpdate()
	{
		Vector2 MovementVector = new Vector2(0, 1);//REVIEW: maybe make this Vector2 a variable that can be modified in engine
															//Or, Im guessing you just want to move forward, so maybe you just want to use the player's forward vector
		moveController.SetMovementVector(MovementVector);
		LedgeCheck();
		if (isGoingToDie)
		{
			End();
		}
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
		isCharging = true;
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
		if (isCharging)
			End();
	}

	void End()
	{
		GetComponent<PlayerInput>().SwitchCurrentActionMap("PlayerMovement");
		moveController.SetMovementVector(Vector2.zero);
		moveController.SetMaxMoveSpeed(maxMoveSpeedTemp);
		isCharging = false;
		//agent.enabled = false;
		//rb.isKinematic = false;
	}
	public void OnStopCharge(InputAction.CallbackContext context)
	{
		End();
		moveController.OnJump(context);
	}

	void LedgeCheck()
	{
		Debug.Log("ledgeChecking");
		// shamelessly stolen from enemy ai which was shamelessly stolen from playermovement TODO: combine groundchecks
		bool ledgeCheck = false;
		bool wallCheck = false;

		//set ground check
		ledgeCheck = Physics.Raycast(transform.position + transform.forward * forwardCliffOffset, Vector3.down, forwardCliffCheckDistance, groundLayer);

		wallCheck = Physics.Raycast(transform.position + transform.forward * 1.3f, transform.forward, wallCheckDistance, groundLayer);

		isGoingToDie = !ledgeCheck || wallCheck;
	}
	/*
	private void OnDrawGizmos()
	{
		Gizmos.DrawLine(transform.position + transform.forward * forwardCliffOffset, transform.position + (transform.forward * forwardCliffOffset) + Vector3.down * forwardCliffCheckDistance);
		Gizmos.DrawLine(transform.position + transform.forward * wallCheckOffset, transform.position + transform.forward * wallCheckOffset + transform.forward * wallCheckDistance);
	}
	*/
	private void OnTriggerEnter(Collider other)
	{
		Damageable targetHealth = other.GetComponent<Damageable>();
		if (other.tag == "Enemy")
		{
			DealDamage(targetHealth);
		}
	}
	private void OnCollisionEnter(Collision collision)
	{
		Damageable targetHealth = collision.gameObject.GetComponent<Damageable>();
		if (collision.gameObject.tag == "Enemy")
		{
			DealDamage(targetHealth);
		}
	}

	public void DealDamage(Damageable targetHealth)
	{
		// make sure it has health to be damaged
		if (targetHealth != null && isCharging)
		{
			targetHealth.TakeDamage(ramAttack, transform.forward);
		}
	}
}