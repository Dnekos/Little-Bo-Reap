using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonCredits : MonoBehaviour
{
    [SerializeField] List<GameObject> objectsToEnable;
    [SerializeField] List<GameObject> objectsToDisable;


    public void CreditsButton()
    {
        for(int i = 0; i < objectsToEnable.Count; i++)
        {
            objectsToEnable[i].SetActive(true);
        }
        for (int i = 0; i < objectsToDisable.Count; i++)
        {
            objectsToDisable[i].SetActive(false);
        }
    }

}
