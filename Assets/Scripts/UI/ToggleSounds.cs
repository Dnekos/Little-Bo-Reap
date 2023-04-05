using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleSounds : MonoBehaviour
{
    [SerializeField] FMODUnity.EventReference hoverSound;
    [SerializeField] FMODUnity.EventReference activationSound;
    [SerializeField] FMODUnity.EventReference deactivationSound;
    ProgressionParent progression;
    [SerializeField] Camera camera;
    [SerializeField] GameObject gameObject;
    private void Start()
    {
    
       // FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Mouse", camera.WorldToScreenPoint(gameObject).x / Screen.currentResolution.width);
        progression = FindObjectOfType<ProgressionParent>();
    }
    public void OnHover()
    {
        FMODUnity.RuntimeManager.PlayOneShot(hoverSound, transform.position);
    }
    public void OnClick()
    {
        FMODUnity.RuntimeManager.PlayOneShot(activationSound, transform.position);
        /*if (!progression.altAbilityUnlocked)
        {
            FMODUnity.RuntimeManager.PlayOneShot(activationSound, transform.position);

        }
        if(progression.altAbilityUnlocked)
        {
            FMODUnity.RuntimeManager.PlayOneShot(deactivationSound);
        }*/
    }
}
