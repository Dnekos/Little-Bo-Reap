using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartButton : MonoBehaviour
{
    [SerializeField] string levelToLoad;
	[SerializeField] string endlessLevelToLoad;
	[SerializeField] string bossLevel;

	[Header("Buttons"), SerializeField]
	Button Continue;

	[Header("SaveData")]
	[SerializeField] TextAsset BossSaveData;

    [Header("Effects"), SerializeField] List<PlayerSheepAI> menuSheep;
    [SerializeField] GameObject gothExplosion;
    [SerializeField] GameObject gothVolume;
    [SerializeField] PlayerGothMode theBoPeeper;
    [SerializeField] Animator boPeeperAnimator;

	[Header("Sounds")]
	[SerializeField] FMODUnity.EventReference clickSound;

	private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

		// disable continue button if there is no save data
		Continue.gameObject.SetActive(WorldState.SaveExists());
	}

	/// <summary>
	/// continue based on save data
	/// </summary>
	public void StartGame()
	{
		StartEffects();
		int index = WorldState.GetLastLevel();

		// if we got real save data, load the right scene, else load what we normally would
		if (index != -1)
			StartCoroutine(LoadSceneByIndex(index));
		else
			Invoke("LoadScene", 2f);
	}
	public void StartNewGame()
	{
		StartEffects();
		WorldState.DeleteSave();
		Invoke("LoadScene", 2f);
	}

	void StartEffects()
	{
		FMODUnity.RuntimeManager.PlayOneShot(clickSound);

		gothVolume.SetActive(true);
		for (int i = 0; i < menuSheep.Count; i++)
		{
			Instantiate(gothExplosion, menuSheep[i].transform.position, menuSheep[i].transform.rotation);
			menuSheep[i].GothMode();
			boPeeperAnimator.Play("Bo_Peep_Summon");
			theBoPeeper.SetGothVisual();
		}

		// disable buttons to prevent doubleclicking
		Button[] buttons = GetComponentsInChildren<Button>();
		for (int i = 0; i < buttons.Length;i++)
		{
			buttons[i].interactable = false;
		}
	}

	void LoadScene()
    {
        SceneManager.LoadScene(levelToLoad);
    }
	IEnumerator LoadSceneByIndex(int index)
	{
		yield return new WaitForSeconds(2);
		SceneManager.LoadScene(index);
	}

	public void LoadEndless()
    {
		StartEffects();
		levelToLoad = endlessLevelToLoad;
		Invoke("LoadScene", 2f);
	}

	public void LoadBoss()
    {
		WorldState.OverwriteSave(BossSaveData);
		StartGame();
	}
}
