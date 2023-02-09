using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewVortex", menuName = "ScriptableObjects/Vortex")]
public class SheepVortexBehavior : SheepBehavior
{
	[Header("Vortex Set Up")]
	[SerializeField] float defendSpeed = 35f;
	[SerializeField] float defendStopDistance = 0f;

	[Header("Vortex")]
	[SerializeField] SheepAttack defendAttack;
	[SerializeField] float vortexRadius = 8f;

	[Header("Randomness")]
	[SerializeField] float vortexRandCircle = 5f;
	[SerializeField] float vortexMinHeight = 0f;
	[SerializeField] float vortexMaxHeight = 2f;

	[Header("Vortex Speed")]
	[SerializeField] float inVortexRotSpeed = 5f;
	[SerializeField] float inVortexLerpSpeed = 0.6f;
	[SerializeField] bool inVortexLerpUseDt = false;

	public override void AbilityUpdate(PlayerSheepAI ps)
	{

		Transform player = WorldState.instance.player.transform;
		Debug.Log("following player");

		if (Vector3.Distance(player.transform.position, ps.transform.position) < vortexRadius + 2)//defendRotateDistance - 2f)
		{
			float radAngle = (ps.sheepPoolIndex / (float)ps.activeSheepPool.Count) * Mathf.PI * 2;
			Vector2 RandomCircle = Random.insideUnitCircle.normalized * vortexRandCircle;

			Vector3 dest = player.transform.position
				+ new Vector3(RandomCircle.x, Random.Range(vortexMinHeight, vortexMaxHeight), RandomCircle.y)
				+ new Vector3(Mathf.Sin(radAngle + Time.time * inVortexRotSpeed), 0, Mathf.Cos(radAngle + Time.time * inVortexRotSpeed)) * vortexRadius ;

			ps.transform.position = Vector3.Lerp(ps.transform.position, dest, inVortexLerpSpeed * (inVortexLerpUseDt ? Time.deltaTime : 1));
		}
		else
		{
			ps.transform.position = Vector3.Lerp(ps.transform.position, player.transform.position, inVortexRotSpeed * Time.deltaTime);
		}
	}

	public override void Begin(PlayerSheepAI ps, Vector3 targettedPos)
	{
		float radAngle = (ps.sheepPoolIndex / (float)ps.activeSheepPool.Count) * Mathf.PI * 2;
		Debug.Log("sheep at index: " + ps.sheepPoolIndex + " has angle of " + radAngle);

		//
		Debug.Log("SO begin");
		ps.agent.enabled = false;

		float randAnimSpeed = Random.Range(1f, 3f);
		ps.GetAnimator().speed = randAnimSpeed;

		ps.GetAnimator().SetBool("isDefending", true);

		//set speed
		ps.agent.speed = defendSpeed;
		ps.agent.stoppingDistance = defendStopDistance;
		ps.SetSheepState(SheepStates.VORTEX);
	}

	public override void End(PlayerSheepAI ps, GameObject fluffyProjectile)
	{
		PlayerSheepProjectile launchSheep = Instantiate(fluffyProjectile, ps.transform.position, ps.transform.rotation).GetComponent<PlayerSheepProjectile>();
		if (ps.isBlackSheep)
			launchSheep.isBlackSheep = true;
		launchSheep.LaunchProjectile(ps.transform.position - WorldState.instance.player.transform.position);
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

