using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepGrave : Interactable
{
    [Header("Sheep Grave Variables")]
    [SerializeField] SheepTypes sheepType;
    [SerializeField] int flockSizeIncrease;

	[Header("Effects")]
	[SerializeField] FMODUnity.EventReference Sound;
	[SerializeField] ParticleSystem graveParticles;
    [SerializeField] GameObject graveLight;
    [SerializeField] ParticleSystem gravePoof;


    public override void Interact()
    {
        //increase flock size of player
        GameManager.Instance.GetPlayer().GetComponent<PlayerSheepAbilities>().sheepFlocks[(int)sheepType].MaxSize += flockSizeIncrease;
        GameManager.Instance.GetPlayer().GetComponent<PlayerSheepAbilities>().UpdateFlockUI();

        FMODUnity.RuntimeManager.PlayOneShotAttached(Sound, gameObject);
		graveParticles.Stop(true);
        graveLight.SetActive(false);
        gravePoof.Play(true);
        base.Interact();
    }
}
