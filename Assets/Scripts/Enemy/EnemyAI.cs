using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using XNode.Examples.StateGraph;

public enum EnemyStates
{
	ACTIVE = 0,
	HITSTUN = 1,
	EXECUTABLE = 2,
}
public class EnemyAI : EnemyBase
{
	[Header("AI"), SerializeField]
	StateGraph graph;
	Dictionary<int, float> Cooldowns;

	//This info is now in EnemyBase
	//[Header("Spawning")]
	//public GameObject SpawnParticlePrefab;
	//public float SpawnWaitTime = 2;

	[Header("Enemy State")]
	[SerializeField] protected EnemyStates currentEnemyState;
	[SerializeField] float StunTime = 0.3f;
	[SerializeField] LayerMask playerLayer;

	[Header("Attacking")]
	public List<Transform> NearbyGuys;
	protected EnemyAttack activeAttack;
	[SerializeField, Tooltip("how frequently the enemy checks their behavior tree")] float delayBetweenAttacks = 1;
	[SerializeField] Collider StickCollider;
	[SerializeField] Animator anim;

	[Header("Bell")]
	public bool distracted;
	public Vector3 bellLoc;

	[Header("Execution Variables")]
	public bool isExecutable;
	public bool mustBeExecuted;
	[SerializeField] protected int executionHealthThreshhold;
	[SerializeField] protected GameObject executeTrigger;
	public Transform executePlayerPos;
	public Execution execution;
	[SerializeField] float timeTillGib = 10f;
	EnemyStates stunState;

	[Header("Ground Check")]
	[SerializeField] LayerMask groundLayer;
	[SerializeField] Transform groundCheckOriginFront;
	[SerializeField] Transform groundCheckOriginBack;
	[SerializeField] float groundCheckDistance;
	public bool isGrounded = false;
	[SerializeField] float fallRate = 50;

	[Header("Sounds")]
	[SerializeField] FMODUnity.EventReference swingSound;
	[SerializeField] FMODUnity.EventReference clubHitSound;


	[HideInInspector] public Transform player;
	NavMeshAgent agent;

	// Start is called before the first frame update
	override protected void Start()
	{
		base.Start();
		agent = GetComponent<NavMeshAgent>();
		player = WorldState.instance.player.transform;
		Cooldowns = new Dictionary<int, float>();
		NearbyGuys = new List<Transform>();
		InvokeRepeating("RunBehaviorTree", delayBetweenAttacks, delayBetweenAttacks);
	}
	void RunBehaviorTree()
	{
		NearbyGuys.RemoveAll(item => item == null || !item.gameObject.activeInHierarchy);
		graph.AnalyzeGraph(this);
	}
	// Update is called once per frame
	protected virtual void Update()
	{
		if (anim != null)
			anim.SetBool("isMoving", agent.velocity.magnitude > 1);

		if (agent.desiredVelocity.sqrMagnitude > 0.8f)
		{
			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(agent.desiredVelocity, Vector3.up), 0.2f);
			transform.eulerAngles = new Vector3(0, transform.eulerAngles.y);
		}
		foreach (var key in Cooldowns.Keys.ToList())
			Cooldowns[key] -= Time.deltaTime;
	}
	private void FixedUpdate()
	{
		//apply gravity if falling
		if (currentEnemyState == EnemyStates.HITSTUN || currentEnemyState == EnemyStates.EXECUTABLE)
			rb.AddForce(Vector3.down * fallRate);
	}
	#region UtilityFunctions
	public NavMeshAgent GetAgent()
	{
		return agent;
	}
	public EnemyStates GetState()
	{
		return currentEnemyState;
	}
	public Animator GetAnimator()
	{
		return anim;
	}
	public virtual bool SetDestination(Vector3 dest)
	{
		// dont pathfind bad destinations
		if (dest == null || float.IsNaN(dest.x) || dest == Vector3.negativeInfinity || dest == Vector3.positiveInfinity)
		{
			Debug.LogWarning("tried giving " + gameObject + " invalid destination");
			return false;
		}
		if (!agent.isOnNavMesh && !agent.isOnOffMeshLink)
		{
			print(gameObject + " failed to find a destination");
			base.OnDeath();
			return false;
		}
		else
		{
			agent.SetDestination(dest);
			Debug.Log(dest);

			// if on offmeshlink, no mdification needed, if the mesh link is too tall they may through an error
			if (agent.isOnOffMeshLink)
				return true;
			if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5, 1))
			{
				transform.position = hit.position;
			}
			else
			{
				print(gameObject + " tried finding a destination while not on a valid point");
				base.OnDeath();
				return false;
			}
			//transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dest - transform.position, Vector3.up), 0.2f);
			//transform.eulerAngles = new Vector3(0, transform.eulerAngles.y);
		}
		return true;
	}
	#endregion

	#region Execution 
	public void Execute()
	{
		base.OnDeath();
	}
	#endregion

	#region Attacking
	Vector3 ClosestGuy()
	{
		float dist = Mathf.Infinity;
		Vector3 closestPos = NearbyGuys[0].position;
		for (int i = 0; i < NearbyGuys.Count; i++)
		{
			float newdist = (NearbyGuys[i].position - transform.position).sqrMagnitude;
			if (newdist < dist)
			{
				dist = newdist;
				closestPos = NearbyGuys[i].position;
			}
		}
		return closestPos;
	}
	public bool RunAttack(EnemyAttack atk)
	{
		int index = convert4(atk.ID);
		if (!Cooldowns.ContainsKey(index))
			Cooldowns.Add(index, -1);
		if (Cooldowns[index] < 0)
		{
			atk.PerformAttack(anim);
			FMODUnity.RuntimeManager.PlayOneShotAttached(swingSound, gameObject);
			Cooldowns[index] = atk.MaxCooldown;
			activeAttack = atk;
			return true;
		}
		return false;
	}

	public void PlaySound(string path)
	{
		FMODUnity.RuntimeManager.PlayOneShotAttached(path, gameObject);
	}
	#endregion

	#region Collisions
	private void OnCollisionEnter(Collision collision)
	{
		// double check that the collision is due to the attack
		if (collision.GetContact(0).thisCollider == StickCollider)
		{
			Debug.Log("collided with " + collision.gameObject.name);
			Damageable hitTarget = collision.gameObject.GetComponent<Damageable>();
			if (hitTarget != null)
			{
				collision.gameObject.GetComponent<Damageable>()?.TakeDamage(activeAttack, transform.forward);
				FMODUnity.RuntimeManager.PlayOneShotAttached(clubHitSound, gameObject);
			}
		}
	}
	private void OnTriggerEnter(Collider other)
	{
		Damageable target = other.GetComponent<Damageable>();
		if (target != null && !(target is EnemyBase) && !other.isTrigger && !NearbyGuys.Contains(other.transform))
		{
			NearbyGuys.Add(other.transform);
		}
	}
	private void OnTriggerExit(Collider other)
	{
		Damageable target = other.GetComponent<Damageable>();
		if (target != null && !(target is EnemyBase) && !other.isTrigger && NearbyGuys.Contains(other.transform))
		{
			NearbyGuys.Remove(other.transform);
		}
	}
	#endregion

	int convert4(string key)
	{
		// https://stackoverflow.com/questions/3858908/convert-a-4-char-string-into-int32
		return (key[3] << 24) + (key[2] << 16) + (key[1] << 8) + key[0];
	}
	#region Movement
	void GroundCheck()
	{
		Debug.Log("ground checking");
		// shamelessly stolen from playermovement TODO: combine groundchecks
		bool frontCheck = false;
		bool backCheck = false;

		//set ground check
		frontCheck = Physics.Raycast(groundCheckOriginFront.position, Vector3.down, groundCheckDistance, groundLayer);

		backCheck = Physics.Raycast(groundCheckOriginBack.position, Vector3.down, groundCheckDistance, groundLayer);

		isGrounded = frontCheck || backCheck;
	}
	#endregion

	#region Health Override and Hitstun
	//to apply normal damage, use this overload
	public override void TakeDamage(Attack atk, Vector3 attackForward, float damageAmp = 1, float knockbackMultiplier = 1)
	{
		//if they must be executed, return
		if (mustBeExecuted && Health < executionHealthThreshhold)
			return;
		// give them hitstun
		if (atk.DealsHitstun)
		{
			StopAllCoroutines();
			StartCoroutine("OnHitStun");
		}
		// subtract health
		base.TakeDamage(atk, attackForward, damageAmp, knockbackMultiplier);

		if (isExecutable && Health <= executionHealthThreshhold)
			ToExecutionState();
	}

	protected override void OnDeath()
	{
		if (isExecutable)
			ToExecutionState();
		else
			base.OnDeath();
	}

	void ToExecutionState()
	{
		suckResistant = true;
		rb.mass = 1f;
		rb.velocity = Vector3.zero;
		rb.useGravity = true;
		agent.enabled = false;
		rb.isKinematic = false;
		gameObject.layer = LayerMask.NameToLayer("EnemyExecute");
		rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
		currentEnemyState = EnemyStates.EXECUTABLE;
		executeTrigger.SetActive(true);
	}

	IEnumerator ForceGibTimer()
	{
		yield return new WaitForSeconds(timeTillGib);
		ForceKill();
	}

	public override void ForceKill()
	{
		isExecutable = false;
		base.ForceKill();
	}
	/*//to apply black sheep damage, use this overload
	public override void TakeDamage(SheepAttack atk, Vector3 attackForward)
	{
		//if they must be executed, return
		if (mustBeExecuted && Health < executionHealthThreshhold)
			return;
		// give them hitstun
		if (atk.dealsHitstunBlack)
		{
			//StopCoroutine("OnHitStun");
			StopAllCoroutines();
			StartCoroutine(OnHitStun());
		}
		// subtract health
		base.TakeDamage(atk, attackForward);
		if (Health <= executionHealthThreshhold && isExecutable)
		{
			rb.mass = 100f;
			rb.velocity = Vector3.zero;
			agent.enabled = false;
			gameObject.layer = LayerMask.NameToLayer("EnemyExecute");
			rb.constraints = RigidbodyConstraints.FreezeAll;
			currentEnemyState = EnemyStates.EXECUTABLE;
			executeTrigger.gameObject.SetActive(true);
		}
	}*/
	IEnumerator OnHitStun()
	{
		// save current state and set to Hitstun
		stunState = (currentEnemyState == EnemyStates.HITSTUN) ? stunState : currentEnemyState;
		currentEnemyState = EnemyStates.HITSTUN;
		//turn on rb and turn off navmesh (turned on in GroundCheck (which cant be called when hitstunned))
		//rb.isKinematic = false;
		agent.enabled = false;
		rb.constraints = RigidbodyConstraints.None;
		rb.constraints = RigidbodyConstraints.FreezeRotation;
		yield return new WaitForSeconds(StunTime);
		// stay in stun until touching the ground
		do
		{
			yield return new WaitForSeconds(0.01f);
			GroundCheck();
		} while (!isGrounded);

		//reset if not in execute stage
		//Demetri this is a quick n dirty fix might need to move around execute stuff eventually
		if (currentEnemyState != EnemyStates.EXECUTABLE)
			currentEnemyState = stunState;

		//rb.isKinematic = true;
		agent.enabled = true;

		// freeze dammit
		rb.constraints = RigidbodyConstraints.FreezeAll;
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
	}
	#endregion
}