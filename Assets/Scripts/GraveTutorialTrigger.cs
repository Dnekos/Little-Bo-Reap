using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GraveTutorialTrigger : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI tutorialText;
    [TextArea(15, 20)]
    [SerializeField] string newTutorialText;
    [SerializeField] SheepGrave theGrave;
    [SerializeField] bool spawnsEnemies;
    [SerializeField] List<GameObject> enemiesToSpawn;
    [SerializeField] GameObject spawnPoof;
    bool hasChanged = false;

    [Header("UI")]
    [SerializeField] bool enablesUI;
    [SerializeField] List<GameObject> panelsToEnable;


    private void Update()
    {
        //change tutorial text when grave is interacted
        if(!theGrave.canInteract && !hasChanged)
        {
            tutorialText.text = newTutorialText;

            //spawn enemies 
            if(spawnsEnemies)
            {
                for (int i = 0; i < enemiesToSpawn.Count; i++)
                {
                    Instantiate(spawnPoof, enemiesToSpawn[i].transform.position, Quaternion.identity);
                }
                Invoke("SpawnEnemies", 2f);
            }

            //enable ui elements
            if(enablesUI)
            {
                for (int i = 0; i < panelsToEnable.Count; i++)
                {
                    Debug.Log("Panel enabled");
                    panelsToEnable[i].SetActive(true);
                }
            }

            hasChanged = true;
        }
    }

    private void SpawnEnemies()
    {
        for (int i = 0; i < enemiesToSpawn.Count; i++)
        {
            enemiesToSpawn[i].SetActive(true);
        }
    }
}
