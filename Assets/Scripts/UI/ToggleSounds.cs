using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleSounds : MonoBehaviour
{
    [SerializeField] FMODUnity.EventReference hoverSound;
    [SerializeField] FMODUnity.EventReference activationSound;
    [SerializeField] FMODUnity.EventReference deactivationSound;
    ProgressionParent progression;
    private void Start()
    {
        progression = FindObjectOfType<ProgressionParent>();
    }
    public void OnHover()
    {
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Mouse", Input.mousePosition.x / Screen.currentResolution.width);
        FMODUnity.RuntimeManager.PlayOneShot(hoverSound, transform.position);
    }
    public void OnClick()
    {
        Debug.Log(Input.mousePosition.x / Screen.currentResolution.width);
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Mouse", Input.mousePosition.x / Screen.currentResolution.width);
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
