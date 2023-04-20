using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerSoulCounter : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI soulText;
    [SerializeField] int soulAmount;
    [SerializeField] FMODUnity.EventReference soulSFX;

    // Start is called before the first frame update
    void Start()
    {
        soulText.SetText("Souls: {0}", soulAmount);
    }

    public void incrementSouls(int soulValue)
    {
        soulAmount += soulValue;
        soulText.SetText("Souls: {0}", soulAmount);
        FMODUnity.RuntimeManager.PlayOneShot(soulSFX);
    }
}

