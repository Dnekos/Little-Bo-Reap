using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSounds : MonoBehaviour
{
    [SerializeField] FMODUnity.EventReference hoverSound;
    [SerializeField] FMODUnity.EventReference clickSound;
    public void OnHover()
    {
        //Debug.Log(Input.mousePosition.x / Screen.currentResolution.width);
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Mouse", Input.mousePosition.x/ Screen.currentResolution.width);
        FMODUnity.RuntimeManager.PlayOneShot(hoverSound, transform.position);
    }
    public void OnClick()
    {
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Mouse", Input.mousePosition.x / Screen.currentResolution.width);
        FMODUnity.RuntimeManager.PlayOneShot(clickSound, transform.position);
    }
    public void MusicEnable()
    
    {
        FMOD.Studio.Bus musicBUs = FMODUnity.RuntimeManager.GetBus("bus:/Music");
        //musicBUs.setPaused(false);
    }
}
