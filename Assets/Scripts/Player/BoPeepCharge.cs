using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BoPeepCharge : MonoBehaviour
{
	[Header("Stampeding")]
	[SerializeField] float chargeSpeed = 35f;
	[SerializeField] float chargePointRadius = 10f;
	[SerializeField] float chargeStopDistance = 0f;
	[SerializeField] float destinationDist = 20f;

	[Header("End Condition")]
	[SerializeField] float chargeCheckTime = 1f;
	[SerializeField] float chargeCheckSpeed = 2f;

	[Header("Attack and Effects")]
	[SerializeField] SheepAttack chargeAttack;
	[SerializeField] GameObject chargeExplosion;

	[SerializeField] NavMeshAgent agent;
	[SerializeField] Rigidbody rb;

	[SerializeField] bool charging;

	private void FixedUpdate()
	{
		if(charging)
		{
			AbilityUpdate();
		}
	}

	public void AbilityUpdate()
	{
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
	}

	public void Begin(Vector3 targettedPos)
	{
		agent.enabled = true;
		rb.isKinematic = true;
		charging = true;
		//set timer to 0
		this.StartCoroutine(ChargeTimer());

		//set destination
		//targettedPos.y = this.transform.position.y;
		this.transform.LookAt(targettedPos + this.transform.position);
		AbilityUpdate();

		//set charge speed
		agent.speed = chargeSpeed;

		agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;		
	}

	IEnumerator ChargeTimer()
	{
		yield return new WaitForSeconds(chargeCheckTime);
		if (charging && agent.velocity.magnitude <= chargeCheckSpeed)
			End();
	}

	void End()
	{
		charging = false;
		agent.enabled = false;
		rb.isKinematic = false;
	}
}
