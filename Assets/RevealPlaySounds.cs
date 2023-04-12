using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevealPlaySounds : MonoBehaviour
{

    [SerializeField] FMODUnity.EventReference creaking;
    [SerializeField] FMODUnity.EventReference jumping;
    [SerializeField] FMODUnity.EventReference explosion;

    public void PlaySound(string path)
    {
        Debug.Log("Played " + path);
        FMODUnity.RuntimeManager.PlayOneShotAttached(path, gameObject);
    }
}
