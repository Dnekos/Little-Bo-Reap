using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GraveTutorialTrigger : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI tutorialText;
    [SerializeField] string newTutorialText;
    [SerializeField] SheepGrave theGrave;
    bool hasChanged = false;

    private void Update()
    {
        if(!theGrave.canInteract && !hasChanged)
        {
            tutorialText.text = newTutorialText;
        }
    }
}
