using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSummoningResource : MonoBehaviour
{
    [Header("Summoning Mana")]
    [SerializeField] float maxBlood;
    [SerializeField] float currentBlood;
    [SerializeField] float bloodMeterRechargePerSec = 10f;
	[SerializeField] FillBar bar;

	[Header("Respawning")]
	[SerializeField]
	GameEvent RespawnEvent;

	private void Start()
	{
		RespawnEvent.listener.AddListener(delegate { ResetBlood(); });
		bar.ChangeFill(currentBlood / maxBlood);
	}

	void ResetBlood()
	{
		currentBlood = maxBlood;
		bar.ChangeFill(1);
	}

	public float GetCurrentBlood()
    {
        return currentBlood;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentBlood >= maxBlood) 
			currentBlood = maxBlood;
		else
		{
			currentBlood += Time.deltaTime * bloodMeterRechargePerSec;
			bar.ChangeFill(currentBlood / maxBlood);
		}
	}

	public void UpdateMeter()
    {
		bar.ChangeFill(currentBlood / maxBlood);
	}

	public void ChangeBloodAmount(float theAmount)
    {
        currentBlood += theAmount;
    }
}
