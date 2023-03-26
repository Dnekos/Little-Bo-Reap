using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReticleControls : MonoBehaviour
{
    [SerializeField] GameObject reticle;
    Color baseColor;
    [SerializeField] int fadeTimeInFrames;
    int duration;

    // Start is called before the first frame update
    void Start()
    {
        //grabs the starting color (read: opacity) of the reticle for later use, then makes it transparent.
        baseColor = reticle.GetComponent<Image>().color;
        reticle.GetComponent<Image>().color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (duration <= 0)
        {
            reticle.GetComponent<Image>().color = new Color(baseColor.r, baseColor.g, baseColor.b, reticle.GetComponent<Image>().color.a - 0.1f);
        }
        else
        {
            duration--;
        }
    }

    //Resets Reticle to full opacity full timer
    public void ResetReticle()
    {
        reticle.GetComponent<Image>().color = baseColor;
        duration = fadeTimeInFrames;
    }
}
