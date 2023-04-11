using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReticleControls : MonoBehaviour
{
	[SerializeField] GameEvent ResetAimMode, EndAimMode;
    //REVIEW: This should probably be private and serialized
    public List<Sprite> reticles;
    
    [SerializeField] GameObject reticle;
    Color baseColor;
    [SerializeField] float fadeTimeInSec, alphaFadeSpeed = 1;
    float duration;

	Image im;

    public void SetReticule(SheepTypes sheepType)
    {
        im.sprite = reticles[(int)sheepType];
    }

    // Start is called before the first frame update
    void Start()
    {
		ResetAimMode.Add(ResetReticle);
		EndAimMode.Add(EndReticle);

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
			im.color = new Color(baseColor.r, baseColor.g, baseColor.b, im.color.a - alphaFadeSpeed * Time.deltaTime);
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
	public void EndReticle()
	{
		im.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0.0f);
		duration = 0;
	}
}
