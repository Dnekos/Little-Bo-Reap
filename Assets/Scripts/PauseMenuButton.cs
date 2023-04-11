using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PauseMenuButton : MonoBehaviour
{
    [SerializeField] PlayerPauseMenu pp; //hehe
    [SerializeField] GameObject panel;
	[SerializeField] GameObject FirstSelected;

	

	public void ResumeGame()
	{
		pp.PauseGame();
		FMOD.Studio.Bus sfxBus = FMODUnity.RuntimeManager.GetBus("bus:/SFX/Gameplay");
		FMOD.Studio.Bus musicBus = FMODUnity.RuntimeManager.GetBus("bus:/Music");
		musicBus.setPaused(false);
		sfxBus.setPaused(false);
	}

	public void EnableOrDisablePanel()
	{
		if (panel.activeSelf)
		{
			panel.SetActive(false);
		}
		else
		{
			panel.SetActive(true);
		}

	}
	private void OnEnable()
	{
		EventSystem.current.SetSelectedGameObject(FirstSelected);
	}
}
