using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerGothMode : MonoBehaviour
{
    [Header("Goth Mode")]
    [SerializeField] GameObject gothParticles;
    [SerializeField] GameObject explosion;
    [SerializeField] AudioClip gothSound;
    [SerializeField] Image gothMeterFill;
    [SerializeField] float gothMeterChargeTime;
    [SerializeField] float gothMeterDuration;
    public bool isGothMode = false;

    PlayerSheepAbilities playerSheep;
    AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
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
            audioSource.PlayOneShot(gothSound);

            Instantiate(explosion, transform.position, transform.rotation);

            isGothMode = true;
            gothParticles.SetActive(true);
            playerSheep.GoGothMode();
        }
    }
}
