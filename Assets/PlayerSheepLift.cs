using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSheepLift : MonoBehaviour
{
	[SerializeField] float TimeBetweenMoves = 0.1f;
	[SerializeField] float InterpolationCycles = 10;
	[SerializeField] float SheepSpacingMod = 0.5f;
	float sheepHeight;

	List<Vector3> RecordedPositions;
	Vector3 lastSheepSpawn;

	PlayerSheepAbilities flocks;
	PlayerMovement player;

	int usedSheep = 0;

	private void Start()
	{
		player = GetComponent<PlayerMovement>();
		flocks = GetComponent<PlayerSheepAbilities>();
		sheepHeight = flocks.GetCurrentSheepPrefab(SheepTypes.BUILD).GetComponentInChildren<CapsuleCollider>().height;
		RecordedPositions = new List<Vector3>();
	}

	// Update is called once per frame
	void Update()
    {
		//flocks.GetCurrentSheepFlock(SheepTypes.BUILD)

		for (int i = 0; i < RecordedPositions.Count - 1; i++)
		{
			Debug.DrawLine(RecordedPositions[i], RecordedPositions[i + 1], i % 2  == 0 ? Color.red : Color.green);
			Debug.DrawLine(RecordedPositions[i], RecordedPositions[i] + Vector3.forward,Color.blue);

		}
	}
	public void StartLifting()
	{
		
		//lastSheepSpawn = transform.position - Vector3.up * GetComponent<CapsuleCollider>().height * 0.5f;
	}
	public IEnumerator PlayerPath()
	{
		RecordedPositions = new List<Vector3>();
		lastSheepSpawn = transform.position;// - Vector3.up * GetComponent<CapsuleCollider>().height * 0.5f;
		usedSheep = 0;

		while (player.Lifting)
		{
			//yield return new WaitForSeconds(TimeBetweenMoves);
			yield return new WaitForEndOfFrame();
			//Debug.Log("enumerating yo + "+ (usedSheep)+ " >= "+ flocks.GetCurrentSheepFlock(SheepTypes.BUILD).Count);

			if (usedSheep >= flocks.GetCurrentSheepFlock(SheepTypes.BUILD).Count)
			{
				player.Lifting = false;
				break;
			}

			// add new point on line
			RecordedPositions.Add(transform.position);//GetComponent<Rigidbody>().position);

			// check if theres room to add a sheep to the stack
			if (Vector3.Distance( RecordedPositions[RecordedPositions.Count-1],lastSheepSpawn) * SheepSpacingMod >= sheepHeight )
			{
				//yield return new WaitForEndOfFrame();
				StartCoroutine(SheepFollow(flocks.GetCurrentSheepFlock(SheepTypes.BUILD)[usedSheep++], 0, usedSheep == 1));
				lastSheepSpawn = RecordedPositions[RecordedPositions.Count - 1];

			}
		}

	}

	IEnumerator SheepFollow(PlayerSheepAI playerSheep,int startingIndex, bool debug = false)
	{
		playerSheep.StartLift(); 
		int index = startingIndex;
		float startTime = Time.time;
		while (player.Lifting)
		{

			//yield return new WaitForFixedUpdate(TimeBetweenMoves * InterpolationCycles);
			yield return new WaitForEndOfFrame();

			//float delta = Time.time - startTime;
			//float t = delta / TimeBetweenMoves ;
			//if (debug)
			//	Debug.Log("index:"+index + " d:" + delta + " t:" + t);
			//Debug.Log(x * InterpolationCycles + " " +x + " " + TimeBetweenMoves);
			playerSheep.transform.position = Vector3.LerpUnclamped(RecordedPositions[index], RecordedPositions[index + 1], 1);
			//if (t >= 1)
			//{
				//startTime = Time.time - (1-delta);
				index++;

			//}
		}
		//playerSheep.KillSheep();
	}
}
