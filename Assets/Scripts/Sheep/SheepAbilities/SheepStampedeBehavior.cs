using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "NewStampede", menuName = "ScriptableObjects/Stampede")]
public class SheepStampedeBehavior : SheepBehavior
{
	[Header("Stempeding")]
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


	public override void AbilityUpdate(PlayerSheepAI ps)
	{
		//set destination
		//get random point inside radius
		Vector3 chargePosition = ps.transform.position + ps.transform.forward * destinationDist;

		//if inside navmesh, charge!
		if (NavMesh.SamplePosition(chargePosition, out NavMeshHit hit, chargePointRadius, 1))
		{
			//get charge
			chargePosition = hit.position;

			//set agent destination
			ps.agent.destination = chargePosition;
		}
		else
		{
			if (ps.leaderSheep == ps)
				Debug.Log("didn't find a chargepoint");
			ps.agent.destination = chargePosition;

			// end charge
			ps.SetSheepState(SheepStates.WANDER);
			ps.chargeParticles.SetActive(false);
		}
	}

	public override void Begin(PlayerSheepAI ps, Vector3 targettedPos)
	{
		//CHARGE!
		ps.chargeParticles.SetActive(true);

		//set timer to 0
		ps.StartCoroutine(ChargeTimer(ps));

		//set destination
		//targettedPos.y = ps.transform.position.y;
		ps.transform.LookAt(targettedPos + ps.transform.position);
		AbilityUpdate(ps);

		ps.agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

		//set sheep state
		ps.SetSheepState(SheepStates.STAMPEDE);
	}

	IEnumerator ChargeTimer(PlayerSheepAI ps)
	{
		yield return new WaitForSeconds(chargeCheckTime);
		if (ps.GetSheepState() == SheepStates.STAMPEDE && ps.agent.velocity.magnitude <= chargeCheckSpeed)
			EndCharge(ps);
	}

	void EndCharge(PlayerSheepAI ps)
	{
		//if (ps.leaderSheep == ps)
		//	Debug.Log("stopping charge " + (chargeCheckCurrent > chargeCheckTime) + " " + (agent.velocity.magnitude <= chargeCheckSpeed));

		ps.SetSheepState(SheepStates.WANDER);
		ps.chargeParticles.SetActive(false);
        ps.agent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;

    }

    public override void AbilityTriggerEnter(PlayerSheepAI ps, Collider other)
	{
		if (other.CompareTag("Enemy"))
		{
			Instantiate(chargeExplosion, ps.transform.position, ps.transform.rotation);
			ps.DealDamage(other, chargeAttack, ps.isBlackSheep);
			ps.TakeDamage(ps.selfDamage, ps.transform.forward);
		}
		if (other.CompareTag("Breakable"))
		{
			other.GetComponent<BreakableWall>()?.DamageWall();
		}
	}
	public override void End(PlayerSheepAI ps, GameObject obj) { }
}