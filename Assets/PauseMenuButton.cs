using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuButton : MonoBehaviour
{
    [SerializeField] PlayerPauseMenu pp; //hehe
    [SerializeField] GameObject panel;

	public void ResumeGame()
    {
        pp.PauseGame();
    }

    public void EnableOrDisablePanel()
    {
        if(panel.activeSelf)
        {
            panel.SetActive(false);
        }
        else
        {
            panel.SetActive(true);
        }
        
    }
}
