using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewLevelTrigger : MonoBehaviour
{
    [SerializeField] string levelToLoad;
	[SerializeField] FMODUnity.EventReference EndSFX;

	// hye michael, why couldnt we use the old level transition script :(

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
			// save game
			if (WorldState.instance != null)
				WorldState.instance.SetSaveNextLevel(SceneManager.GetSceneByName(levelToLoad).buildIndex);

			FMOD.Studio.Bus myBus = FMODUnity.RuntimeManager.GetBus("bus:/SFX");
			myBus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
			FMODUnity.RuntimeManager.PlayOneShot(EndSFX);
			SceneManager.LoadScene(levelToLoad);
		}
    }
}
