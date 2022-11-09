using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldState : MonoBehaviour
{
	public static WorldState instance;

	public bool Dead = false;

	[SerializeField]
	Checkpoint[] SpawnPoints;
	[SerializeField]
	int activeSpawnPoint = -1;
	//List<BattleArena> completedArenas;

	[SerializeField] GameEvent Respawn;

	public float currentScore = 0;

	float checkedScore = 0;
	int deaths = 0;


	public GameObject player;

	// Start is called before the first frame update
	void Awake()
	{
		if (instance == null)
		{
			instance = this;
			Respawn.listener.AddListener(delegate { RespawnPlayer(); });
			player = GameObject.FindGameObjectWithTag("Player");

			Debug.Log("made World Instance");
			//StopSong.Post(gameObject);
			//PlaySong.Post(gameObject);
		}
	}

	public void SetSpawnPoint(Checkpoint point)
	{
		activeSpawnPoint = System.Array.FindIndex<Checkpoint>(SpawnPoints, spawnpoint => spawnpoint == point);
		checkedScore = currentScore;
	}

	void RespawnPlayer()
	{
		if (activeSpawnPoint == -1)
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
			return;
		}

		Debug.Log(SpawnPoints[activeSpawnPoint] + " " + player +  " " + player.transform.position);

		Cursor.lockState = CursorLockMode.Locked;
		Dead = false;

		player.GetComponent<Rigidbody>().position = SpawnPoints[activeSpawnPoint].RespawnPoint.position;
		player.transform.rotation = SpawnPoints[activeSpawnPoint].RespawnPoint.rotation;
		player.GetComponent<Rigidbody>().velocity = Vector3.zero;

		Debug.Log(SpawnPoints[activeSpawnPoint] + " " + player +" "+ player.transform.position);

		player.GetComponent<PlayerSheepAbilities>().DeleteAllSheep();


		deaths++;
		currentScore = checkedScore;
	}

}
