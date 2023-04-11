using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject controlMenu;
    [SerializeField] FMODUnity.EventReference pauseSFX;
    [SerializeField] FMODUnity.EventReference resumeSFX;
    public PlayerInput inputs;
    bool isPaused = false;

    //TODO
    //make this better, fix edge cases, prevent inputs in game etc bla bla bla
    public void OnPauseMenu(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        Debug.Log("pause");
        isPaused = !isPaused;

		FMOD.Studio.Bus myBus = FMODUnity.RuntimeManager.GetBus("bus:/SFX/Gameplay");

		if (isPaused)
        {
			myBus.setPaused(true);
			inputs.SwitchCurrentActionMap("PauseMenu");
            pauseMenu.SetActive(true);
            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            FMODUnity.RuntimeManager.PlayOneShot(pauseSFX);
        }
        else
        {
			myBus.setPaused(false);

			inputs.SwitchCurrentActionMap("PlayerMovement");
			controlMenu.SetActive(false);
            pauseMenu.SetActive(false);
            Time.timeScale = 1f;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            FMODUnity.RuntimeManager.PlayOneShot(resumeSFX);
        }
    }
}
