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
		//increase flock size of player
		PlayerSheepAbilities player = WorldState.instance.player.GetComponent<PlayerSheepAbilities>();
		player.sheepFlocks[(int)sheepType].MaxSize += flockSizeIncrease;
		player.UpdateFlockUI();

		//show flock size increase
		TextMeshProUGUI number = Instantiate(numberObject, numberSpawnPoint.position, numberSpawnPoint.rotation).GetComponentInChildren<TextMeshProUGUI>();
		number.color = numberColor;
        number.text = "+" + (flockSizeIncrease).ToString();

		// juice
        FMODUnity.RuntimeManager.PlayOneShotAttached(Sound, gameObject);
		graveParticles.Stop(true);
        graveLight.SetActive(false);
        gravePoof.Play(true);

        inputIcon.gameObject.SetActive(false);

        base.Interact();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == WorldState.instance.player && canInteract)
            inputIcon.gameObject.SetActive(true);

    }
	protected override void OnTriggerExit(Collider other)
    {
        if (other.gameObject == WorldState.instance.player)
            inputIcon.gameObject.SetActive(false);
    }
}
