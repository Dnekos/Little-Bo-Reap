using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulCollectableScript : Collectable
{
    GameObject soulCounter;
    [SerializeField] int soulValue;
	[SerializeField] float healingValue = 3;
    // Start is called before the first frame update
    void Start()
    {
       //soulCounter = GameObject.FindGameObjectWithTag("SoulCounter");
    }
    //effect: adds souls equal to value and updates the UI
    protected override void CollectableEffect()
    {
		//soulCounter.GetComponent<PlayerSoulCounter>().incrementSouls(soulValue);
		Debug.Log("OverrideSuccessful");

		playerBody.GetComponentInChildren<PlayerHealth>().Heal(healingValue);
    }
}
