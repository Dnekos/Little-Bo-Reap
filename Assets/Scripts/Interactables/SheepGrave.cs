using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SheepGrave : Interactable
{
    [Header("Sheep Grave Variables")]
    [SerializeField] SheepTypes sheepType;
    [SerializeField] int flockSizeIncrease;

    [Header("Sounds")]
	[SerializeField] FMODUnity.EventReference Sound;

	[Header("Particles")]
	[SerializeField] ParticleSystem graveParticles;
    [SerializeField] GameObject graveLight;
    [SerializeField] ParticleSystem gravePoof;

	[Header("UI")]
	[SerializeField] Transform numberSpawnPoint;
    [SerializeField] GameObject numberObject;
    [SerializeField] Color numberColor;

    public override void Interact()
    {
		// increase flock size of player
		PlayerSheepAbilities player = WorldState.instance.player.GetComponent<PlayerSheepAbilities>();
		player.sheepFlocks[(int)sheepType].MaxSize += flockSizeIncrease;
		player.UpdateFlockUI();

		// show flock size increase
		TextMeshProUGUI number = Instantiate(numberObject, numberSpawnPoint.position, numberSpawnPoint.rotation).GetComponentInChildren<TextMeshProUGUI>();
		number.color = numberColor;
        number.text = "+" + (flockSizeIncrease).ToString();

		// update save, just do all sheep as its easier
		WorldState.instance.AddActivatedGrave(this);
		WorldState.instance.PersistentData.totalBuilder = player.sheepFlocks[0].MaxSize;
		WorldState.instance.PersistentData.totalRam = player.sheepFlocks[1].MaxSize;
		WorldState.instance.PersistentData.totalFluffy = player.sheepFlocks[2].MaxSize;


		// juice
		FMODUnity.RuntimeManager.PlayOneShotAttached(Sound, gameObject);
		graveParticles.Stop(true);
        graveLight.SetActive(false);
        gravePoof.Play(true);

        inputIcon.gameObject.SetActive(false);

        base.Interact();
    }
	/// <summary>
	/// Turns off the grave visually. Sheep totals are saved seperately.
	/// </summary>
	public override void InteractBackend()
	{
		// turn juice off
		graveParticles.Stop(true);
		graveLight.SetActive(false);
		inputIcon.gameObject.SetActive(false);

		base.Interact();
	}

	/*
	protected override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == WorldState.instance.player && inputIcon != null && canInteract)
            inputIcon.gameObject.SetActive(true);

    }
	protected override void OnTriggerExit(Collider other)
    {
        if (other.gameObject == WorldState.instance.player && inputIcon != null && canInteract)
            inputIcon.gameObject.SetActive(false);
    }
	*/
}
