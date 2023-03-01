using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[System.Serializable]
public struct SheepPassives
{
	[Header("Soul Count")]
	public int soulsCount;
	public int bossSoulsCount;

	[Header("Active Abilities")]
	public string activeBuilderAbility;
	public string activeRamAbility;
	public string activeFluffyAbility;

	[Header("Builder Upgrades")]
	public float builderLaunchDam;
	public float builderCorruptChance;
	public float builderConstructDR;

	[Header("Ram Upgrades")]
	public float ramChargeDR;
	public float ramDamage;
	public float ramKnockback;

	[Header("Fluffy Upgrades")]
	public float fluffyHealth;
	public float fluffyKnockResist;
	public float fluffyVortexDuration;

	
}

public class WorldState : MonoBehaviour
{
	public static WorldState instance;
	public enum State
	{
		Play,
		Dead,
		Dialog
	}

	public State gameState = State.Play;
	
	//TODO unserialize this to make inpsector look clean
	[SerializeField] public SheepPassives passiveValues;
	

	[SerializeField]
	Checkpoint[] SpawnPoints;
	[SerializeField]
	int activeSpawnPoint = -1;

	[SerializeField] GameEvent Respawn;

	public float currentScore = 0;
	float checkedScore = 0;

	[Header("Cinematic"), SerializeField] GameEvent MusicToggle;
	bool UIOff = false;
	FMODUnity.StudioEventEmitter music;
	public int currentWorldTheme;


	// values other classes may care about
	int deaths = 0;
	public List<GameObject>[] SheepPool;

	// providing components
	[HideInInspector]
	public GameObject player;
	[HideInInspector]
	public HUDManager HUD;



	// Start is called before the first frame update
	void Awake()
	{
		if (instance == null)
		{
			// set up instance
			instance = this;
			Respawn.listener.AddListener(RespawnPlayer);
			player = GameObject.FindGameObjectWithTag("Player");

			Debug.Log("made World Instance");

			MusicToggle.listener.AddListener(Toggle);
			music = GetComponent<FMODUnity.StudioEventEmitter>();
			ChangeMusic(0);
			currentWorldTheme = 0;

			// default settings
			PlayerCameraFollow cam = FindObjectOfType<PlayerCameraFollow>();
			if (cam != null)
				cam.mouseSensitivity = PlayerPrefs.GetFloat("sensitivity", 1) * cam.mouseSensitivityMax;

			// volume settings
			FMOD.Studio.Bus myBus = FMODUnity.RuntimeManager.GetBus("bus:/SFX");
			myBus.setVolume(PlayerPrefs.GetFloat("sfx", 1));
			myBus = FMODUnity.RuntimeManager.GetBus("bus:/Music");
			myBus.setVolume(PlayerPrefs.GetFloat("music", 1));

			SheepPool = new List<GameObject>[3];
			SheepPool[0] = new List<GameObject>();
			SheepPool[1] = new List<GameObject>();
			SheepPool[2] = new List<GameObject>();
		}
		else if (instance != this)
			Destroy(gameObject);


	}
	public void ChangeMusic(int music)
    {
		FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Music", music);
		Debug.Log("current theme: "+music);
	}
	void Toggle()
	{
		UIOff = !UIOff;
		if (UIOff)
			music.Stop();
		else
			music.Play();
	}

	#region Spawning
	public void SetSpawnPoint(Checkpoint point)
	{
		activeSpawnPoint = System.Array.FindIndex<Checkpoint>(SpawnPoints, spawnpoint => spawnpoint == point);
		checkedScore = currentScore;
	}

	public void OnSpawnNextSpawnPoint(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			activeSpawnPoint = Mathf.Min(activeSpawnPoint + 1, SpawnPoints.Length - 1);
			Respawn.listener.Invoke();
		}
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
		gameState = State.Play;

		player.GetComponent<Rigidbody>().position = SpawnPoints[activeSpawnPoint].RespawnPoint.position;
		player.transform.rotation = SpawnPoints[activeSpawnPoint].RespawnPoint.rotation;
		player.GetComponent<Rigidbody>().velocity = Vector3.zero;

		Debug.Log(SpawnPoints[activeSpawnPoint] + " " + player +" "+ player.transform.position);

		deaths++;
		currentScore = checkedScore;
	}
	#endregion

}
