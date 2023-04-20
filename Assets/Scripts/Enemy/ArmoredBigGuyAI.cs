using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ArmoredBigGuyAI : BigGuyAI
{
    [Header("Armor")]
	[SerializeField] GameObject ArmorBarUI;
    [SerializeField] GameObject armorObject;
    [SerializeField] ParticleSystem destroyParticles;
    [SerializeField] ParticleSystem armorSparks;
    [SerializeField] FMODUnity.EventReference armorBreakSFX;
    [SerializeField] FMODUnity.EventReference armorReflectSFX;
    private bool armorBroken = false;
    private bool armorRecentlyBroken = false;

    protected override void Start()
    {
        base.Start();

		ArmorBarUI.SetActive(true);
		HealthBarCanvas.SetActive(true);
	}

	public override void TakeDamage(Attack atk, Vector3 attackForward, float damageAmp = 1, float knockbackMultiplier = 1)
    {
        if(armorBroken == true && armorRecentlyBroken == false)
        {
            base.TakeDamage(atk, attackForward, damageAmp, 0.0f);//no knockback
            //REVIEW: I would use >= for the health execution if check
            if (Health != MaxHealth || Health >= executionHealthThreshhold)
            {
                HealthBarCanvas.SetActive(true);
                float healthbarScale = (Health / MaxHealth);
				HPBar.fillAmount = healthbarScale;

			}
			else
                HealthBarCanvas.SetActive(false);
        }
        else if((atk.name == "Ram_Attack_Charge" || atk.name == "HammerAttack") && armorBroken == false)
        {
			ArmorBarUI.SetActive(false);
            HealthBarCanvas.SetActive(true);
            armorObject.SetActive(false);
            destroyParticles.Play(true);
            armorBroken = true;
            FMODUnity.RuntimeManager.PlayOneShot(armorBreakSFX, transform.position);
            //start I-Frames
            StartCoroutine(ShieldRecentlyBroken());

            //armor break sound
        }
        else if ((atk.name != "Ram_Attack_Charge" || atk.name != "HammerAttack") && armorBroken == false)
        {
            FMODUnity.RuntimeManager.PlayOneShot(armorReflectSFX, transform.position);
        }
        //add a section for attacks that dont break shield for the sound 
        else if(armorBroken == false)
        {
            //Sparks and armor hit sound
            Instantiate(armorSparks, armorObject.transform);

            //this would be an attack that doesnt break the shield
            //Debug.Log(Time.time % 10f);
            //if (Time.deltaTime % 0.2f == 0)
            //{
            //    Debug.Log("armor hit");
            //    
            //}
        }
    }

    public IEnumerator ShieldRecentlyBroken()
    {
        armorRecentlyBroken = true;
        yield return new WaitForSeconds(0.5f);
        armorRecentlyBroken = false;
    }
}
