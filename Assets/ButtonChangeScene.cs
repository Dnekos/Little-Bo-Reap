using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonChangeScene : MonoBehaviour
{
    [SerializeField] string levelToLoad;
    public void ChangeScene()
    {
        SceneManager.LoadScene(levelToLoad);
    }
}
