using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PauseMenuButton : MonoBehaviour
{
    [SerializeField] PlayerPauseMenu pp; //hehe
    [SerializeField] GameObject panel;
	[SerializeField] GameObject FirstSelected;
	[Header("Sounds")]
	[SerializeField] FMODUnity.EventReference clickSound;

	public void ResumeGame()
	{
		FMODUnity.RuntimeManager.PlayOneShot(clickSound);
		pp.PauseGame();
		FMOD.Studio.Bus myBus = FMODUnity.RuntimeManager.GetBus("bus:/SFX/Gameplay");
		myBus.setPaused(false);

	}

	public void EnableOrDisablePanel()
	{
		FMODUnity.RuntimeManager.PlayOneShot(clickSound);

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
