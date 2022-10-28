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


    private void Update()
    {
        if(!theGrave.canInteract && !hasChanged)
        {
            tutorialText.text = newTutorialText;

            if(spawnsEnemies)
            {
                for (int i = 0; i < enemiesToSpawn.Count; i++)
                {
                    Instantiate(spawnPoof, enemiesToSpawn[i].transform.position, Quaternion.identity);
                }
                Invoke("SpawnEnemies", 1f);
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
