using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleSounds : MonoBehaviour
{
    [SerializeField] FMODUnity.EventReference hoverSound;
    public void OnHover()
    {
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Mouse", Input.mousePosition.x / Screen.currentResolution.width);
        FMODUnity.RuntimeManager.PlayOneShot(hoverSound, transform.position);
    }
}
