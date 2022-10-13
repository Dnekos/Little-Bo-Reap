using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSheepLift : MonoBehaviour
{
	[SerializeField] float TimeBetweenMoves = 0.1f;
	[SerializeField] float InterpolationCycles = 10;
	[SerializeField] float SheepSpacingMod = 0.5f;
	[SerializeField] float Step;
	[SerializeField] CapsuleCollider mainCollider;
	float sheepHeight;

	List<Vector3> RecordedPositions;
	Vector3 lastSheepSpawn;

	PlayerSheepAbilities flocks;
	Rigidbody rb;
	PlayerMovement player;

	float distTowardsNextSheep = 0;
	int usedSheep = 0;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		player = GetComponent<PlayerMovement>();
		flocks = GetComponent<PlayerSheepAbilities>();
		sheepHeight = flocks.GetCurrentSheepPrefab(SheepTypes.BUILD).GetComponentInChildren<CapsuleCollider>().height;
		RecordedPositions = new List<Vector3>();
	}

	// Update is called once per frame
	void Update()
    {

		for (int i = 0; i < RecordedPositions.Count - 1; i++)
		{
			Debug.DrawLine(RecordedPositions[i], RecordedPositions[i + 1], i % 2  == 0 ? Color.red : Color.green);
			Debug.DrawLine(RecordedPositions[i], RecordedPositions[i] + Vector3.forward,Color.blue);
		}

	}
	public bool StartLifting()
	{
		RaycastHit info;
		Physics.Raycast(transform.position, Vector3.down, out info, 100, LayerMask.GetMask("Ground"));
		Debug.Log("p:" + info.point + " d:" + info.distance);

		RecordedPositions = new List<Vector3>();
		lastSheepSpawn = info.point;
		usedSheep = 0;
		player.Lifting = true;

		float interval = player.LiftSpeed * Time.deltaTime;
		for (float i = info.point.y; i < transform.position.y - (mainCollider.height * 0.5f); i += interval)
		{
			if (!PlacePoint(new Vector3(info.point.x, i, info.point.z), true))
			{
				player.Lifting = false;
				return false;
			}
		}

		return true;
	}

	// returns false if you cannot place a sheep
	bool PlacePoint(Vector3 newPos, bool PlaceAtTop)
	{
		if (usedSheep >= flocks.GetCurrentSheepFlock(SheepTypes.BUILD).Count)
		{
			player.Lifting = false;
			return false;
		}

		// add new point on line
		RecordedPositions.Add(newPos);

		if (RecordedPositions.Count > 2)
			distTowardsNextSheep += Vector3.Distance(RecordedPositions[RecordedPositions.Count - 1], RecordedPositions[RecordedPositions.Count - 2]);

		// check if theres room to add a sheep to the stack
		if (distTowardsNextSheep * SheepSpacingMod >= sheepHeight)
		{
			StartCoroutine(SheepFollow(flocks.GetCurrentSheepFlock(SheepTypes.BUILD)[usedSheep++], PlaceAtTop ? RecordedPositions.Count - 1 : 0, usedSheep == 1));
			lastSheepSpawn = RecordedPositions[RecordedPositions.Count - 1];

			distTowardsNextSheep = 0;
		}
		return true;
	}

	public IEnumerator PlayerPath()
	{
		while (player.Lifting)
		{
			yield return new WaitForEndOfFrame();

			if (!PlacePoint(transform.position - Vector3.up * (mainCollider.height * 0.5f), false))
				break;
		}
	}

	IEnumerator SheepFollow(PlayerSheepAI playerSheep,int startingIndex, bool debug = false)
	{
		playerSheep.StartLift(); 
		int index = startingIndex;
		float startTime = Time.time;
		while (player.Lifting)
		{
			yield return new WaitForEndOfFrame();

			playerSheep.transform.position = RecordedPositions[index];// Vector3.LerpUnclamped(RecordedPositions[index], RecordedPositions[index + 1], 1);
				index++;
		}
	}
}
