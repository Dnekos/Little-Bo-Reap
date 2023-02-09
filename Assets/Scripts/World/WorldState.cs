using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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

	// values other classes may care about
	int deaths = 0;
	//public int maxBuilderSheep = 0, maxRamSheep = 0, maxFluffySheep = 0;

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

			// default settings
			PlayerCameraFollow cam = FindObjectOfType<PlayerCameraFollow>();
			if (cam != null)
				cam.mouseSensitivity = PlayerPrefs.GetFloat("sensitivity", 1) * cam.mouseSensitivityMax;

			// volume settings
			FMOD.Studio.Bus myBus = FMODUnity.RuntimeManager.GetBus("bus:/SFX");
			myBus.setVolume(PlayerPrefs.GetFloat("sfx", 1));
			myBus = FMODUnity.RuntimeManager.GetBus("bus:/Music");
			myBus.setVolume(PlayerPrefs.GetFloat("music", 1));
		}
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
