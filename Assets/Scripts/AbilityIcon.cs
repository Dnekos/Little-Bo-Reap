using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class AbilityIcon : MonoBehaviour
{
    float rechargeTime = 1f;
    Image image;
    bool isFilling = false;


    private void Start()
    {
        image = GetComponent<Image>();   
    }
    private void Update()
    {
        //i know this is sloppy but whatever, needed to test this
        if(isFilling)
        {
            image.fillAmount += 1.0f / rechargeTime * Time.unscaledDeltaTime;

            if (image.fillAmount >= 1) isFilling = false;
        }
        
    }

    public void CooldownUIEffect(float theCooldown)
    {
        isFilling = true;
        image.fillAmount = 0f;
        rechargeTime = theCooldown;
    }
}
