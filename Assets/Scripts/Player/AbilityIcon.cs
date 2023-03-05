using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class AbilityIcon : MonoBehaviour
{
    float rechargeTime = 1f;
    Image image;
    public Image Image { get { return image; } }

    bool isFilling = false;

    public event Action<float> fillableChanged;

    private void Start()
    {
        image = GetComponent<Image>();   
    }
    private void Update()
    {
        //i know this is sloppy but whatever, needed to test this
        if(isFilling)
        {
            image.fillAmount += rechargeTime * Time.unscaledDeltaTime;

            if (image.fillAmount >= 1)
				isFilling = false;

            fillableChanged.Invoke(image.fillAmount);
        }
    }

    public void IsEnabled()
	{
        image.fillAmount = 1;
	}

    public void CooldownUIEffect(float theCooldown)
    {
        isFilling = true;
        image.fillAmount = 0f;
        rechargeTime = 1.0f / theCooldown;
    }
}
