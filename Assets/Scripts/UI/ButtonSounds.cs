using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSounds : MonoBehaviour
{
    [SerializeField] FMODUnity.EventReference hoverSound;
    [SerializeField] FMODUnity.EventReference clickSound;
    private void Start()
    {
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Mouse", transform.position.x / Screen.currentResolution.width);
    }
    public void OnHover()
    {
        FMODUnity.RuntimeManager.PlayOneShot(hoverSound, transform.position);
    }
    public void OnClick()
    {
        FMODUnity.RuntimeManager.PlayOneShot(clickSound, transform.position);
    }
}
