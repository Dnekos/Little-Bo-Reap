using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class AbilityIcon : MonoBehaviour
{
    float rechargeTime = 1f;
    Image image;

    private void Start()
    {
        image = GetComponent<Image>();   
    }
    private void Update()
    {
        //i know this is sloppy but whatever, needed to test this
        image.fillAmount += 1.0f / rechargeTime * Time.unscaledDeltaTime;
    }

    public void CooldownUIEffect(float theCooldown)
    {
        image.fillAmount = 0f;
        rechargeTime = theCooldown;
    }
}
