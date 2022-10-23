using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSummoningResource : MonoBehaviour
{
    [Header("Summoning Mana")]
    [SerializeField] float maxBlood;
    [SerializeField] float currentBlood;
    [SerializeField] Image bloodMeter;
    [SerializeField] float bloodMeterRechargePerSec = 10f;
	[SerializeField] FillBar bar;

    public float GetCurrentBlood()
    {
        return currentBlood;
    }

    // Update is called once per frame
    void Update()
    {
        currentBlood += Time.deltaTime * bloodMeterRechargePerSec;
        if (currentBlood >= maxBlood) currentBlood = maxBlood;
        UpdateMeter();
		bar.ChangeFill(currentBlood / maxBlood);

	}
    
    public void UpdateMeter()
    {
        bloodMeter.fillAmount = currentBlood / maxBlood;
    }

    public void ChangeBloodAmount(float theAmount)
    {
        currentBlood += theAmount;
    }
}
