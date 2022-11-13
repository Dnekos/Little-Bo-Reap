using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    [SerializeField] string levelToLoad;
    [SerializeField] List<PlayerSheepAI> menuSheep;
    [SerializeField] GameObject gothExplosion;
    [SerializeField] GameObject gothVolume;
    [SerializeField] PlayerGothMode theBoPeeper;
    [SerializeField] Animator boPeeperAnimator;

    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void StartGame()
    {
        gothVolume.SetActive(true);
        for(int i = 0; i < menuSheep.Count; i++)
        {
            Instantiate(gothExplosion, menuSheep[i].transform.position, menuSheep[i].transform.rotation);
            menuSheep[i].GothMode();
            boPeeperAnimator.Play("Bo_Peep_Summon");
            theBoPeeper.ForceGothMode();
        }
        Invoke("LoadScene", 2f);
    }

    void LoadScene()
    {
        SceneManager.LoadScene(levelToLoad);
    }
}
