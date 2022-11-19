using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonChangeScene : MonoBehaviour
{
	[SerializeField] string levelToLoad;

    [Header("Sounds")]
	[SerializeField] FMODUnity.EventReference clickSound;

	public void ChangeScene()
	{
		FMODUnity.RuntimeManager.PlayOneShot(clickSound);

		// pausing the game pauses gameplay sounds, if we're changing scenes we shouldn't be paused
		FMOD.Studio.Bus myBus = FMODUnity.RuntimeManager.GetBus("bus:/SFX/Gameplay");
		myBus.setPaused(false);

		//dont ask why this is here, it just needs to be
		// unhelpful comment, i think i will ask why this is here
		Time.timeScale = 1f;

		SceneManager.LoadScene(levelToLoad);
	}
}
