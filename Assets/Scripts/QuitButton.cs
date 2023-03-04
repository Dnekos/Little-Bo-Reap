using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitButton : MonoBehaviour
{

    [SerializeField] GameObject quitConfirmMenu;
    public void QuitGame()
    {
        Application.Quit();
    }

    public void EnableQuitConfirm()
    {
        quitConfirmMenu.SetActive(true);
    }

    public void DisableQuitConfirm()
    {
        quitConfirmMenu.SetActive(false);
    }
}
