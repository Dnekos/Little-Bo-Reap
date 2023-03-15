using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LevelEndInteractable : Interactable
{
    [Header("Level to Go To")]
    [SerializeField] string levelName;

    public override void Interact()
    {
		// save game
		if (WorldState.instance != null)
		{
			WorldState.instance.SetSaveNextLevel(SceneManager.GetSceneByName(levelName).buildIndex);
		}

		FMOD.Studio.Bus myBus = FMODUnity.RuntimeManager.GetBus("bus:/SFX");
		myBus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
		SceneManager.LoadScene(levelName);
        base.Interact();
    }
}
