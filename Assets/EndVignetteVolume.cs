using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class EndVignetteVolume : MonoBehaviour
{
	[Header("Postprocess")]
	[SerializeField] VolumeProfile endVolume;
	[SerializeField] float defaultSaturation = -100f;
	[SerializeField] float saturationIncreaseOverTime = 10f;
	UnityEngine.Rendering.Universal.ColorAdjustments endSaturation;
	[SerializeField] Color defaultColorFilter;
	[SerializeField] Color endColorFilter;
	UnityEngine.Rendering.Universal.ColorAdjustments colorFilterVar;
	[SerializeField] float timeToDarken = 5f;
	public bool canDarken = false;
	float timeElapsed = 0f;


    private void Start()
    {
		//get post process stuff
		if (!endVolume.TryGet(out endSaturation))
			throw new System.NullReferenceException(nameof(endSaturation));
		endSaturation.saturation.value = defaultSaturation;

		//get post process stuff
		if (!endVolume.TryGet(out colorFilterVar))
			throw new System.NullReferenceException(nameof(colorFilterVar));
		colorFilterVar.colorFilter.value = defaultColorFilter;
	}

	public void EnableDarken()
    {
		canDarken = true;
    }

    private void Update()
    {
		//deplete saturation over time
		if (endSaturation.saturation.value > -100)
		{
			endSaturation.saturation.value += saturationIncreaseOverTime * Time.deltaTime;
		}

		if(canDarken)
        {
			colorFilterVar.colorFilter.value = Color.Lerp(defaultColorFilter, endColorFilter, timeElapsed / timeToDarken);
			timeElapsed += Time.deltaTime;
		}
		
	}
}
