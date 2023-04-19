using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public enum ActiveUpgrade
{
	Ability1 = 0,
	Ability2 = 1
}

[System.Serializable]
public struct SheepPassives
{
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
[System.Serializable]
public struct SaveData
{
	[System.Flags] public enum Upgrades
	{
		None = 0,
		BuilderLaunchDam = 1,
		BuilderCorruptChance = 2,
		BuilderConstructDR = 4,
		
		RamChargeDR = 8,
		RamDamage = 16,
		RamKnockback = 32,

		FluffyHealth = 64,
		FluffyKnockResist = 128,
		FluffyVortexDuration = 256,

		BuilderActive = 512,
		RamActive = 1024,
		FluffyActive = 2048
	}
	public Upgrades boughtUpgrades;
	public Upgrades activeUpgrades;

	[Header("Soul Count")]
	public int soulsCount;
	public int bossSoulsCount;

	[Header("LevelPosition")]
	public int currentLevelIndex;
	public int currentCheckpoint;
	public bool goingToBoss;

	public List<int> graveIndexes;
	public List<int> doorIndexes;

	[Header("SheepTotals")]
	public int totalBuilder;
	public int totalRam;
	public int totalFluffy;
	public SaveData(int x)
	{
		boughtUpgrades = Upgrades.None;
		activeUpgrades = Upgrades.None;

		soulsCount = 0;
		bossSoulsCount = 0;

		currentLevelIndex = -1;
		currentCheckpoint = -1;
		goingToBoss = false;


		graveIndexes = new List<int>();
		doorIndexes = new List<int>();

		totalBuilder = 0;
		totalRam = 0;
		totalFluffy =0;
	}
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

	[System.Flags]
	public enum LoadState : byte
	{
		None = 0,
		Editor = 1,
		Application = 2
	}
	[SerializeField] LoadState Loading = LoadState.Application;
	//TODO unserialize this to make inpsector look clean
	[SerializeField] public SheepPassives passiveValues;
	public SaveData PersistentData;
	
	// keeping track of objects in the world (mostly for saving)
	[SerializeField]
	Checkpoint[] SpawnPoints;
	[HideInInspector]
	public Interactable[] SheepGraves;
	[HideInInspector]
	public PuzzleDoor[] doors;

	[SerializeField] GameEvent Respawn;

	[Header("Cinematic"), SerializeField] GameEvent MusicToggle;
	bool UIOff = false;
	FMODUnity.StudioEventEmitter music;
	public int currentWorldTheme;
	public int biomeTheme;
	//[SerializeField] FMODUnity.ParamRef boss;

	// values other classes may care about
	int deaths = 0;
	public List<GameObject>[] SheepPool;

	// providing components
	[HideInInspector]
	public GameObject player;
	[HideInInspector]
	public HUDManager HUD;
	[HideInInspector]
	public ParticleObjectPooler pools;

	[Header("Booting player")]
	public bool isInCombat;
	public Transform combatBootPoint;

	// Start is called before the first frame update
	void Awake()
	{
		if (instance == null)
		{
			// set up instance
			instance = this;
			Respawn.Add(RespawnPlayer);
			player = GameObject.FindGameObjectWithTag("Player");
			pools = GetComponent<ParticleObjectPooler>();

			Debug.Log("made World Instance");

			MusicToggle.Add(Toggle);
			music = GetComponent<FMODUnity.StudioEventEmitter>();
			ChangeMusic(0);
			currentWorldTheme = 0;
			biomeTheme = 0;

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

	private void Start()
	{
		FMOD.Studio.Bus musicBus = FMODUnity.RuntimeManager.GetBus("Bus:/Music");
		musicBus.setPaused(false);
		if (Loading != LoadState.None)
			PersistentData = new SaveData(1);
#if UNITY_EDITOR
		if (Loading.HasFlag(LoadState.Editor))
		{
			LoadGame();
		}
#else
		if (Loading.HasFlag(LoadState.Application))
		{
			LoadGame();
		}
#endif
	}

	public void ChangeMusic(int music)
	{
		FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Music", music);
		currentWorldTheme = music;
		Debug.Log("current theme: " + music);
		if (music == 6 || music == 2)
			biomeTheme = 2;
		if (music == 3 || music == 4)
		{
			FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Kick Level", Random.Range(0, 2));
			FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Bass Fades", Random.Range(0f, 2f));
			FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Synth Fade", Random.Range(0f, 2f));
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

	public void DisableControls()
	{
		player.GetComponent<PlayerInput>().DeactivateInput();
	}
	public void EnableControls()
	{
		player.GetComponent<PlayerInput>().ActivateInput();
	}

	#region Spawning
	public void SetSpawnPoint(Checkpoint point)
	{
		PersistentData.currentCheckpoint = System.Array.FindIndex<Checkpoint>(SpawnPoints, spawnpoint => spawnpoint == point);
	}

	public void OnSpawnNextSpawnPoint(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			PersistentData.currentCheckpoint = Mathf.Min(PersistentData.currentCheckpoint + 1, SpawnPoints.Length - 1);

            if (SpawnPoints[PersistentData.currentCheckpoint].addsSheep)
            {
				SpawnPoints[PersistentData.currentCheckpoint].debugSheepAdder.SetActive(true);
			}

			Respawn.Raise();
		}
	}

	public void OnSkiptoBoss(InputAction.CallbackContext context)
    {
		if (context.performed)
		{
			PersistentData.currentCheckpoint = SpawnPoints.Length - 1;
			if (SpawnPoints[PersistentData.currentCheckpoint].addsSheep)
			{
				SpawnPoints[PersistentData.currentCheckpoint].debugSheepAdder.SetActive(true);
			}
			Respawn.Raise();
			Debug.Log("Skip to boss!");
		}
    }

	//tell us we're in combat, sets combat boot point. Called from battleArena
	public void InitCombatBootPoint(Transform spawnLoc)
    {
		isInCombat = true;
		combatBootPoint = spawnLoc;
    }
	public void BootPlayer()
    {
		//in combat you get booted back to the specified point
		if(isInCombat)
        {
			player.GetComponent<Rigidbody>().position = combatBootPoint.position;
			player.transform.rotation = combatBootPoint.rotation;
			player.GetComponent<Rigidbody>().velocity = Vector3.zero;
		}
		//out of combat go to checkpoint
		else
        {
			if (PersistentData.currentCheckpoint == -1)
			{
				SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
				return;
			}
			else
            {
				player.GetComponent<Rigidbody>().position = SpawnPoints[PersistentData.currentCheckpoint].RespawnPoint.position;
				player.transform.rotation = SpawnPoints[PersistentData.currentCheckpoint].RespawnPoint.rotation;
				player.GetComponent<Rigidbody>().velocity = Vector3.zero;
			}
		}
    }

	void RespawnPlayer()
	{
		if (PersistentData.currentCheckpoint == -1)
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
			return;
		}

		Cursor.lockState = CursorLockMode.Locked;
		gameState = State.Play;

		player.GetComponent<Rigidbody>().position = SpawnPoints[PersistentData.currentCheckpoint].RespawnPoint.position;
		player.transform.rotation = SpawnPoints[PersistentData.currentCheckpoint].RespawnPoint.rotation;
		player.GetComponent<Rigidbody>().velocity = Vector3.zero;

		deaths++;
	}
	#endregion

	#region saving Graves and Doors
	public void AddActivatedGrave(Interactable grave)
	{
		int index = System.Array.FindIndex<Interactable>(SheepGraves, spawnpoint => spawnpoint == grave);
		if (!PersistentData.graveIndexes.Contains(index))
			PersistentData.graveIndexes.Add(index);
	}
	public void AddActivatedDoor(PuzzleDoor door)
	{
		int index = System.Array.FindIndex<PuzzleDoor>(doors, spawnpoint => spawnpoint == door);
		if (!PersistentData.doorIndexes.Contains(index))
			PersistentData.doorIndexes.Add(index);
	}
	#endregion

	public void GoToBoss(int levelIndex)
    {
		PersistentData.goingToBoss = true;
		SaveGame(levelIndex);
    }

	#region saving
	// https://www.red-gate.com/simple-talk/development/dotnet-development/saving-game-data-with-unity/
	/// <summary>
	/// saves game and clears now unneeded data (arena and door from old level)
	/// </summary>
	public void SetSaveNextLevel(int levelIndex)
	{
		PersistentData.graveIndexes.Clear();
		PersistentData.doorIndexes.Clear();
		SaveGame(levelIndex);
	}
	public void SaveGame(int levelIndex = -1)
	{
		// set active level
		if (levelIndex == -1)
			PersistentData.currentLevelIndex = SceneManager.GetActiveScene().buildIndex;

		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create(Application.persistentDataPath
					 + "/SaveData.dat");
		bf.Serialize(file, PersistentData);
		file.Close();
		Debug.Log("Game data saved! Saved to "+ Application.persistentDataPath
					 + "/SaveData.dat");
	}
	public void LoadGame()
	{
		string filepath = Application.persistentDataPath + "/SaveData.dat";
		if (File.Exists(filepath))
		{
			// open file
			FileStream file = File.Open(filepath, FileMode.Open);
			LoadDataFromSave(LoadFromStream(file));
			file.Close();
		}
		else
			Debug.LogWarning("There is no save data!");
	}
	public void LoadGame(TextAsset savefile)
	{
		if (savefile != null)
		{
			// open file
			LoadDataFromSave(LoadFromStream(new MemoryStream(savefile.bytes)));
		}
		else
			Debug.LogWarning("There is no save data!");
	}
	void LoadDataFromSave(SaveData data)
	{
		PersistentData = data;

		// set up interactables (targets, pinwheels, arenas, graves)
		for (int i = 0; i < PersistentData.doorIndexes.Count; i++)
			doors[PersistentData.doorIndexes[i]].OpenDoor();
		Debug.Log("graves:" + PersistentData.graveIndexes);
		Debug.Log(SheepGraves.Length);
		for (int i = 0; i < PersistentData.graveIndexes.Count; i++)
			SheepGraves[PersistentData.graveIndexes[i]].InteractBackend();
		Debug.Log("checkpoint:" + PersistentData.currentCheckpoint + " level:" + PersistentData.currentLevelIndex);

		// check if correct scene
		if (PersistentData.currentLevelIndex != SceneManager.GetActiveScene().buildIndex)
			PersistentData.currentCheckpoint = -1;

		// move player if found loaded stuff
		if (PersistentData.currentCheckpoint > -1)
			Respawn.Raise();
	}


	static SaveData LoadFromStream(Stream stream)
	{
		if (stream != null)
		{
			// open file

			BinaryFormatter bf = new BinaryFormatter();
			SaveData data = (SaveData)bf.Deserialize(stream);

			// set up interactables (targets, pinwheels, arenas, graves)
			Debug.Log("Game data loaded!");
			return data;
		}

		Debug.LogWarning("There is no save data!");
		return new SaveData();
	}

	static public void OverwriteSave(TextAsset textAsset)
	{
		SaveData newdata = LoadFromStream(new MemoryStream(textAsset.bytes));

		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create(Application.persistentDataPath
					 + "/SaveData.dat");
		bf.Serialize(file, newdata);
		file.Close();
		Debug.Log("Game data saved! Saved to " + Application.persistentDataPath
					 + "/SaveData.dat");
	}

	static public int GetLastLevel()
	{
		string filepath = Application.persistentDataPath + "/SaveData.dat";

		if (File.Exists(filepath))
		{
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(filepath, FileMode.Open);
			SaveData sp = (SaveData)bf.Deserialize(file);
			file.Close();

			Debug.Log("Game data loaded! last level was " + sp.currentLevelIndex);
			return sp.currentLevelIndex;
		}
		Debug.LogWarning("There is no save data!");
		return -1;
	}

	static public void ClearSave()
	{
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create(Application.persistentDataPath
					 + "/SaveData.dat");
		bf.Serialize(file, new SaveData());
		file.Close();
		Debug.Log("Game data saved! Saved to " + Application.persistentDataPath
					 + "/SaveData.dat");
	}
	static public void DeleteSave()
	{
		string filepath = Application.persistentDataPath
					   + "/SaveData.dat";

		if (File.Exists(filepath))
			File.Delete(filepath);
	}
	static public bool SaveExists()
	{
		string filepath = Application.persistentDataPath
					   + "/SaveData.dat";

		Debug.Log("File existing " + File.Exists(filepath));

		return File.Exists(filepath);
	}
	#endregion
}
