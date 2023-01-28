using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
    [SerializeField] Transform numberSpawnPoint;
    [SerializeField] GameObject numberObject;
    [SerializeField] Color numberColor;




    public override void Interact()
    {
		//increase flock size of player
		WorldState.instance.player.GetComponent<PlayerSheepAbilities>().sheepFlocks[(int)sheepType].MaxSize += flockSizeIncrease;
		WorldState.instance.player.GetComponent<PlayerSheepAbilities>().UpdateFlockUI();

        //show flock size increase
        var number = Instantiate(numberObject, numberSpawnPoint.position, numberSpawnPoint.rotation) as GameObject;
        number.GetComponentInChildren<TextMeshProUGUI>().color = numberColor;
        number.GetComponentInChildren<TextMeshProUGUI>().text = "+" + (flockSizeIncrease).ToString();

        FMODUnity.RuntimeManager.PlayOneShotAttached(Sound, gameObject);
		graveParticles.Stop(true);
        graveLight.SetActive(false);
        gravePoof.Play(true);
        base.Interact();
    }
}
