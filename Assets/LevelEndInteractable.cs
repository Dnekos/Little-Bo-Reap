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
        SceneManager.LoadScene(levelName);
        base.Interact();
    }
}
