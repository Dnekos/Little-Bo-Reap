using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonChangeScene : MonoBehaviour
{
    [SerializeField] string levelToLoad;
    public void ChangeScene()
    {
        //dont ask why this is here, it just needs to be
        Time.timeScale = 1f;

        SceneManager.LoadScene(levelToLoad);
    }
}
