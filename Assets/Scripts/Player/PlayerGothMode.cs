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
    [SerializeField] Image gothMeterFill;
    [SerializeField] float gothMeterChargeTime;
    [SerializeField] float gothMeterDuration;
    public bool isGothMode = false;

    [Header("Postprocess")]
    [SerializeField] float defaultSaturation = -100f;
    [SerializeField] float saturationIncreaseOverTime = 10f;
    UnityEngine.Rendering.Universal.ColorAdjustments gothSaturation;

    PlayerSheepAbilities playerSheep;

    private void Start()
    {
        //get post process stuff
        if (!gothVolume.TryGet(out gothSaturation)) throw new System.NullReferenceException(nameof(gothSaturation));
        gothSaturation.saturation.value = defaultSaturation;

        playerSheep = GetComponent<PlayerSheepAbilities>();
    }

    //update bar image
    void Update()
    {
        if (gothMeterFill.fillAmount <= 0)
        {
            isGothMode = false;
            gothParticles.SetActive(false);
        }


        if (isGothMode)
        {
            //deplete meter
            gothMeterFill.fillAmount -= 1.0f / gothMeterDuration * Time.unscaledDeltaTime;

            //deplete saturation over time
            if(gothSaturation.saturation.value < 0)
            {
                gothSaturation.saturation.value += saturationIncreaseOverTime * Time.deltaTime;
            }
        }
        else
        {
            gothMeterFill.fillAmount += 1.0f / gothMeterChargeTime * Time.unscaledDeltaTime;
        }
    }

    public void OnGothMode(InputAction.CallbackContext context)
    {
        if (context.started && gothMeterFill.fillAmount == 1f)
        {
			//TEMP SOUND
			FMODUnity.RuntimeManager.PlayOneShotAttached(gothSound,gameObject);

            Instantiate(explosion, transform.position, transform.rotation);

            gothSaturation.saturation.value = defaultSaturation;

            isGothMode = true;
            gothParticles.SetActive(true);
            playerSheep.GoGothMode();
        }
    }
}
