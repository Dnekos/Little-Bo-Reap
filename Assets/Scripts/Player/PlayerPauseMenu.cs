using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPauseMenu : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;
    public PlayerInput inputs;
    bool isPaused = false;

    //TODO
    //make this better, fix edge cases, prevent inputs in game etc bla bla bla
    public void OnPauseMenu(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            Debug.Log("pause");
            isPaused = !isPaused;

            if(isPaused)
            {
                inputs.enabled = false;
                pauseMenu.SetActive(true);
                Time.timeScale = 0f;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                inputs.enabled = true;
                pauseMenu.SetActive(false);
                Time.timeScale = 1f;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }
}
