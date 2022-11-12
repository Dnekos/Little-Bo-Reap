using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [SerializeField] GameObject tutorialCanvas;
    [SerializeField] bool enablesPanel;
    [SerializeField] GameObject panelToEnable;

    private void Awake()
    {
        tutorialCanvas.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            tutorialCanvas.SetActive(true);
            if (enablesPanel) panelToEnable.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) tutorialCanvas.SetActive(false);
    }
}
