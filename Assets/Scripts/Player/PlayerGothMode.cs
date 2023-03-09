using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerGothMode : MonoBehaviour
{
    public enum GothState
    {
        Normal = 0,
        Hammer,
        Goth
    }

    [Header("Goth Mode")]
    [SerializeField] GameObject gothParticles;
    [SerializeField] VolumeProfile gothVolume;
    [SerializeField] GameObject explosion;
    [SerializeField] FMODUnity.EventReference gothSound;
	[SerializeField] FillBar gothMeter;
	[SerializeField] float GothMeterCount = 0;
    [SerializeField] float gothMeterChargeTime;
    [SerializeField] float gothMeterDuration;
    public GothState gothMode = GothState.Normal;

	[Header("Hammer")]
	[SerializeField] Interactable sheepHammer;
    [SerializeField] string hammerAnimation = "Po_Peep_Hammer";
    Animator anim;
	PlayerInput input;

    [Header("Postprocess")]
    [SerializeField] float defaultSaturation = -100f;
    [SerializeField] float saturationIncreaseOverTime = 10f;
    UnityEngine.Rendering.Universal.ColorAdjustments gothSaturation;

	[Header("Materials")]
	[SerializeField] GameObject materialParent;
	SkinnedMeshRenderer[] meshes;
	[SerializeField] Material defaultMat;
	[SerializeField] Material gothMat;

	[Header("Respawning")]
	[SerializeField]
	GameEvent RespawnEvent;

	// quick math
	float durationInverse, chargeTimeInverse;

	PlayerSheepAbilities playerSheep;

    private void Start()
    {
        //get post process stuff
        if (!gothVolume.TryGet(out gothSaturation)) 
			throw new System.NullReferenceException(nameof(gothSaturation));
        gothSaturation.saturation.value = defaultSaturation;

        playerSheep = GetComponent<PlayerSheepAbilities>();
		gothMeter.ChangeFill(GothMeterCount);

		RespawnEvent.listener.AddListener(delegate { ResetGoth(); });

		meshes = materialParent.GetComponentsInChildren<SkinnedMeshRenderer>();

		// save these inverses to save calculations
		durationInverse = 1 / gothMeterDuration;
		chargeTimeInverse = 1 / gothMeterChargeTime;

        anim = GetComponent<PlayerAnimationController>().playerAnimator;
		input = GetComponent<PlayerInput>();
    }

	void ResetGoth()
	{
        gothMode = GothState.Normal;
		gothParticles.SetActive(false);

		GothMeterCount = 0;
		gothMeter.ChangeFill(GothMeterCount);
	}

    public void AddToGothMeter(float amount)
    {
        GothMeterCount += amount;
        gothSaturation.saturation.value = defaultSaturation;
    }

	//update bar image
	void Update()
    {
        if (gothMode == GothState.Goth && GothMeterCount <= 0)
        {
            foreach (SkinnedMeshRenderer mesh in meshes)
            {
                mesh.material = defaultMat;
            }
            Instantiate(explosion, transform.position, transform.rotation);

            gothMode = GothState.Normal;
            gothParticles.SetActive(false);


			// turn off hammer
			sheepHammer.Interact();
		}
        else if (gothMode == GothState.Hammer && !anim.GetCurrentAnimatorStateInfo(0).IsName(hammerAnimation))
        {
            SetGothVisual();

			// reenable controls
			input.SwitchCurrentActionMap("PlayerMovement");

			playerSheep.GoGothMode();
        }
        else if (gothMode == GothState.Goth)
        {

			//deplete meter
			GothMeterCount = Mathf.Clamp01(GothMeterCount - (durationInverse * Time.deltaTime));
			gothMeter.ChangeFill(GothMeterCount);

			//deplete saturation over time
			if (gothSaturation.saturation.value < 0)
            {
                gothSaturation.saturation.value += saturationIncreaseOverTime * Time.deltaTime;
            }
        }
        else
        {
			GothMeterCount = Mathf.Clamp01(GothMeterCount + (chargeTimeInverse * Time.deltaTime));
			gothMeter.ChangeFill(GothMeterCount);
		}
    }

    public void OnGothMode(InputAction.CallbackContext context)
    {
        if (context.started && GothMeterCount == 1f)
        {
			input.SwitchCurrentActionMap("Disabled");

            anim.Play(hammerAnimation);

            gothMode = GothState.Hammer;

            sheepHammer.Interact();
		}
	}


    public void SetGothVisual()
    {
        //TEMP SOUND
        FMODUnity.RuntimeManager.PlayOneShotAttached(gothSound, gameObject);

        Instantiate(explosion, transform.position, transform.rotation);

        gothSaturation.saturation.value = defaultSaturation;

        SkinnedMeshRenderer[] mats = materialParent.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer mesh in mats)
        {
            mesh.material = gothMat;
        }


        gothMode = GothState.Goth;
        gothParticles.SetActive(true);
    }
}
