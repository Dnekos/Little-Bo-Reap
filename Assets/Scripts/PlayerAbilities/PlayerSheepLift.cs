using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSheepLift : MonoBehaviour
{
	[SerializeField] int AllowedSurvivingSheep = 5;
	[SerializeField] float SheepSpacingMod = 0.5f;
	[Tooltip("this should be the primary collider on the player"), SerializeField] CapsuleCollider mainCollider;
	float sheepHeight;

	[SerializeField] float SheepLerpSpeed = 0.5f;

	// Platform Stuff
	[Header("Top Platform"), SerializeField] Vector3 PlatformSize;
	GameObject platform;

	// list of points for the sheep to follow
	List<Vector3> RecordedPositions;

	// components
	PlayerSheepAbilities flocks;
	Rigidbody rb;
	PlayerMovement player;


	// internal variables
	float distTowardsNextSheep = 0;
	int usedSheep = 0;
	bool shouldCollapseTower = false;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		player = GetComponent<PlayerMovement>();
		flocks = GetComponent<PlayerSheepAbilities>();
		CapsuleCollider sheepcap = flocks.GetCurrentSheepPrefab(SheepTypes.BUILD).GetComponentInChildren<CapsuleCollider>();
		sheepHeight = sheepcap.height + sheepcap.radius;
		RecordedPositions = new List<Vector3>();

		platform = null;
	}

	// Update is called once per frame
	void Update()
    {

		for (int i = 0; i < RecordedPositions.Count - 1; i++)
		{
			Debug.DrawLine(RecordedPositions[i], RecordedPositions[i + 1], i % 2  == 0 ? Color.red : Color.green);
			Debug.DrawLine(RecordedPositions[i], RecordedPositions[i] + Vector3.forward,Color.blue);
		}

		if (platform != null && player.transform.position.y < platform.transform.position.y)
		{
			CollapseTower();
		}

	}
	public bool StartLifting()
	{
		RaycastHit info;
		bool hitGround = Physics.Raycast(transform.position, Vector3.down, out info, 100, LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore);

		Debug.DrawLine(transform.position, info.point, Color.red, 3);
		Debug.Log("sheep lift attempt at height "+ (info.distance * SheepSpacingMod) +", estimated sheep height is "+ (sheepHeight * flocks.GetSheepFlock(SheepTypes.BUILD).Count));
		
		// not enough sheep to make the ladder
		if (!hitGround || info.distance * SheepSpacingMod > sheepHeight * flocks.GetSheepFlock(SheepTypes.BUILD).Count || info.collider.CompareTag("Sheep") || info.collider.gameObject == platform)
			return false;


		shouldCollapseTower = false;
		RecordedPositions = new List<Vector3>();
		usedSheep = 0;
		player.isLifting = true;

		float interval = player.LiftSpeed * Time.deltaTime;
		for (float i = info.point.y; i < transform.position.y - (mainCollider.height * 0.5f); i += interval)
		{
			if (!PlacePoint(new Vector3(info.point.x, i, info.point.z), true))
			{
				shouldCollapseTower = true;
				player.isLifting = false;

				return false;
			}
		}

		return true;
	}

	public void CollapseTower()
	{
		if (platform != null)
		{
			platform.GetComponent<BoxCollider>().enabled = false;
			platform.layer = 0; // idk, fuckin... make it not ground??
			Destroy(platform);
			platform = null;
		}
		shouldCollapseTower = true;
	}

	#region running lift stuff
	// returns false if you cannot place a sheep
	bool PlacePoint(Vector3 newPos, bool PlaceAtTop)
	{
		if (usedSheep >= flocks.GetSheepFlock(SheepTypes.BUILD).Count)
		{
			player.isLifting = false;
			return false;
		}

		// add new point on line
		RecordedPositions.Add(newPos);

		if (RecordedPositions.Count > 2)
			distTowardsNextSheep += Vector3.Distance(RecordedPositions[RecordedPositions.Count - 1], RecordedPositions[RecordedPositions.Count - 2]);

		// check if theres room to add a sheep to the stack
		if (distTowardsNextSheep * SheepSpacingMod >= sheepHeight)
		{
			StartCoroutine(SheepFollow(flocks.GetSheepFlock(SheepTypes.BUILD)[usedSheep], PlaceAtTop ? RecordedPositions.Count - 1 : 0, usedSheep));

			// give them a new rotation
			flocks.GetSheepFlock(SheepTypes.BUILD)[usedSheep].transform.eulerAngles = new Vector3(0, Random.Range(0, 360));
			usedSheep++;
			distTowardsNextSheep = 0;
		}
		return true;
	}

	public IEnumerator PlayerPath()
	{
		while (player.isLifting)
		{
			yield return new WaitForEndOfFrame();

			if (!PlacePoint(transform.position - Vector3.up * (mainCollider.height * 0.5f), false))
				break;
		}

		platform = new GameObject("Lift Platform");
		platform.transform.position = transform.position - Vector3.up * PlatformSize.y * 1.05f; //RecordedPositions[RecordedPositions.Count - 1];
		platform.layer = LayerMask.NameToLayer("Ground");
		platform.tag = "Sheep";
		platform.AddComponent<BoxCollider>().size = PlatformSize;
	}

	IEnumerator SheepFollow(PlayerSheepAI playerSheep,int startingIndex, int sheepIndex)
	{
		playerSheep.StartLift(); 
		int index = startingIndex;
		float startTime = Time.time;
		float lerpSpeed = SheepLerpSpeed;


		while (player.isLifting && playerSheep != null)
		{
			yield return new WaitForEndOfFrame();

			if (index < RecordedPositions.Count)
			{
				playerSheep.transform.position = Vector3.Lerp(playerSheep.transform.position, RecordedPositions[index], lerpSpeed);
				index++;
				lerpSpeed += Time.deltaTime * 2;
			}
		}

		// snap them in position at the end to catch stragglers
		playerSheep.transform.position = RecordedPositions[Mathf.Clamp(index, 0, RecordedPositions.Count - 1)];

		// keep them in the tower till something knocks them over or they die
		yield return new WaitUntil(() => shouldCollapseTower || playerSheep == null || playerSheep.GetSheepState() != SheepStates.LIFT);

		CollapseTower();

		if (playerSheep?.GetSheepState() == SheepStates.LIFT)
			playerSheep.EndLift(sheepIndex > AllowedSurvivingSheep);
	}
	#endregion

	private void OnCollisionExit(Collision collision)
	{
		if (collision.gameObject == platform)
		{
			player.CanLift = false;
			//CollapseTower();
		}
		else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
			CollapseTower();
	}
}
