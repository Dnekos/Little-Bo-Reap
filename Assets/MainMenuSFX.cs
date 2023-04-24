using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuSFX : MonoBehaviour
{
    FMOD.Studio.Bus myBus;
    // Start is called before the first frame update
    void Start()
    {
        myBus = FMODUnity.RuntimeManager.GetBus("bus:/SFX/Gameplay");

    }

    // Update is called once per frame
    void Update()
    {
        myBus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);

    }
}
