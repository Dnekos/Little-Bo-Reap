using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReticleControls : MonoBehaviour
{
    public List<Sprite> reticles;

    [SerializeField] GameObject reticle;
    Color baseColor;
    [SerializeField] float fadeTimeInSec;
    float duration;

	Image im;

    public void SetReticule(SheepTypes sheepType)
    {
        im.sprite = reticles[(int)sheepType];
    }

    // Start is called before the first frame update
    void Start()
    {
		im = GetComponent<Image>();

		//grabs the starting color (read: opacity) of the reticle for later use, then makes it transparent.
		baseColor = im.color;
		im.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (duration <= 0)
        {
			im.color = new Color(baseColor.r, baseColor.g, baseColor.b, im.color.a - 0.1f);
        }
        else
        {
            duration-= Time.deltaTime;
        }
    }

    //Resets Reticle to full opacity full timer
    public void ResetReticle()
    {
		im.color = baseColor;
        duration = fadeTimeInSec;
    }
}
