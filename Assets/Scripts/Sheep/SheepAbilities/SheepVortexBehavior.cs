using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewVortex", menuName = "ScriptableObjects/Vortex")]
public class SheepVortexBehavior : SheepBehavior
{
	[Header("Defend State Variables")]
	[SerializeField] float defendSpeed = 35f;
	[SerializeField] float defendStopDistance = 0f;
	[SerializeField] SheepAttack defendAttack;
	[SerializeField] float defendRotateDistance = 5f;
	[SerializeField] float defendMinHeight = 0f;
	[SerializeField] float defendMaxHeight = 2f;
	[SerializeField] float defendSlerpTime = 5f;
	Transform defendPoint;
	bool isMovingToDefend;

	public override void AbilityUpdate(PlayerSheepAI ps)
	{
		if (isMovingToDefend)
		{
			Debug.Log("following player");
			ps.transform.position = Vector3.Lerp(ps.transform.position, ps.player.transform.position, defendSlerpTime * Time.deltaTime);

			if (Vector3.Distance(ps.player.transform.position, ps.transform.position) < defendRotateDistance - 2f)
			{
				isMovingToDefend = false;

				ps.transform.parent = defendPoint;
				ps.transform.localPosition = Random.insideUnitCircle.normalized * defendRotateDistance;

				float randPosY = Random.Range(defendMinHeight, defendMaxHeight);

				ps.transform.localPosition = new Vector3(ps.transform.localPosition.x, randPosY, ps.transform.localPosition.y);
			}
		}

	}

	public override void Begin(PlayerSheepAI ps, Vector3 targettedPos)
	{
		//
		ps.agent.enabled = false;

		//set defened mode
		defendPoint = theDefendPoint;
		isMovingToDefend = true;

		float randAnimSpeed = Random.Range(1f, 3f);
		ps.animator.speed = randAnimSpeed;

		ps.animator.SetBool("isDefending", true);

		//set speed
		ps.agent.speed = defendSpeed;
		ps.agent.stoppingDistance = defendStopDistance;
		ps.SetSheepState(SheepStates.VORTEX);
	}

	public void EndDefendPlayer(GameObject fluffyProjectile)
	{
		PlayerSheepProjectile launchSheep = Instantiate(fluffyProjectile, ps.transform.position, ps.transform.rotation).GetComponent<PlayerSheepProjectile>();
		if (ps.isBlackSheep)
			launchSheep.isBlackSheep = true;
		launchSheep.LaunchProjectile(ps.transform.position - ps.player.transform.position);
	}
	public override void AbilityTriggerEnter(PlayerSheepAI ps, Collider other)
	{
		if (other.CompareTag("Enemy"))
		{
			ps.DealDamage(other, defendAttack, ps.isBlackSheep);
			ps.TakeDamage(ps.selfDamage, ps.transform.forward);
		}
		if (other.CompareTag("Pinwheel"))
		{
			Pinwheel pinwheel = other.GetComponent<Pinwheel>();
			if (!pinwheel.isSpinning)
				pinwheel.StartCoroutine(pinwheel.SpinPinwheel());
		}
	}
}

