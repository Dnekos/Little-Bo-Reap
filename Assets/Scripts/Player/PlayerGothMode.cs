using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerGothMode : MonoBehaviour
{
    [Header("Goth Mode")]
    [SerializeField] GameObject gothParticles;
    [SerializeField] VolumeProfile gothVolume;
    [SerializeField] GameObject explosion;
    [SerializeField] FMODUnity.EventReference gothSound;
	[SerializeField] FillBar gothMeter;
	[SerializeField] float GothMeterCount = 0;
    [SerializeField] float gothMeterChargeTime;
    [SerializeField] float gothMeterDuration;
    public bool isGothMode = false;

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
	}

	void ResetGoth()
	{
		isGothMode = false;
		gothParticles.SetActive(false);

		GothMeterCount = 0;
		gothMeter.ChangeFill(GothMeterCount);
	}

	//update bar image
	void Update()
    {
        if (isGothMode && GothMeterCount <= 0)
        {
            foreach (SkinnedMeshRenderer mesh in meshes)
            {
                mesh.material = defaultMat;
            }
            Instantiate(explosion, transform.position, transform.rotation);

            isGothMode = false;
            gothParticles.SetActive(false);
        }


        if (isGothMode)
        {
			//deplete meter
			GothMeterCount = Mathf.Clamp01(GothMeterCount - (1.0f / gothMeterDuration * Time.unscaledDeltaTime));
			gothMeter.ChangeFill(GothMeterCount);

			//deplete saturation over time
			if (gothSaturation.saturation.value < 0)
            {
                gothSaturation.saturation.value += saturationIncreaseOverTime * Time.deltaTime;
            }
        }
        else
        {
			GothMeterCount = Mathf.Clamp01(GothMeterCount + (1.0f / gothMeterChargeTime * Time.unscaledDeltaTime));
			gothMeter.ChangeFill(GothMeterCount);
		}
    }

    public void OnGothMode(InputAction.CallbackContext context)
    {
        if (context.started && GothMeterCount == 1f)
        {
			//TEMP SOUND
			FMODUnity.RuntimeManager.PlayOneShotAttached(gothSound,gameObject);

            Instantiate(explosion, transform.position, transform.rotation);

            gothSaturation.saturation.value = defaultSaturation;

            foreach(SkinnedMeshRenderer mesh in meshes)
            {
                mesh.material = gothMat;
            }


            isGothMode = true;
            gothParticles.SetActive(true);
            playerSheep.GoGothMode();
        }
    }


    public void ForceGothMode()
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


        isGothMode = true;
        gothParticles.SetActive(true);
    }
}