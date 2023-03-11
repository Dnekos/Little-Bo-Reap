using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LevelEndInteractable : Interactable
{
    [Header("Level to Go To")]
    [SerializeField] string levelName;

    [Header("Sound")]
    [SerializeField] FMODUnity.EventReference endSFX;

    public override void Interact()
    {
		FMOD.Studio.Bus myBus = FMODUnity.RuntimeManager.GetBus("bus:/SFX");
		myBus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
        FMODUnity.RuntimeManager.PlayOneShot(endSFX, transform.position);
		SceneManager.LoadScene(levelName);
        base.Interact();
    }
}
