using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ArmoredBigGuyAI : BigGuyAI
{
    [Header("Armor")]
	[SerializeField] GameObject ArmorBarCanvas;
	[SerializeField] Transform[] ArmorBars;
    [SerializeField] GameObject armorObject;
    [SerializeField] ParticleSystem destroyParticles;
    [SerializeField] FMODUnity.EventReference armorBreakSFX;
    private bool armorBroken = false;
    private bool armorRecentlyBroken = false;

    protected override void Start()
    {
        base.Start();

        ArmorBarCanvas.SetActive(true);

        float armorBarScale = (Health / MaxHealth);
        ArmorBars[0].localScale = new Vector3(armorBarScale, 1, 1);
        ArmorBars[1].localScale = new Vector3(armorBarScale * -1, 1, 1);

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
                HPBars[0].localScale = new Vector3(healthbarScale, 1, 1);
                HPBars[1].localScale = new Vector3(healthbarScale * -1, 1, 1);
            }
            else
                HealthBarCanvas.SetActive(false);
        }
        else if(atk.name == "Ram_Attack_Charge" && armorBroken == false)
        {
            ArmorBarCanvas.SetActive(false);
            HealthBarCanvas.SetActive(true);
            armorObject.SetActive(false);
            destroyParticles.Play(true);
            armorBroken = true;
            FMODUnity.RuntimeManager.PlayOneShot(armorBreakSFX, transform.position);
            //start I-Frames
            StartCoroutine(ShieldRecentlyBroken());

            //armor break sound
        }
        //add a section for attacks that dont break shield for the sound 
        else if(armorBroken == false)
        {
            //this would be an attack that doesnt break the shield
            if (Time.deltaTime % 10 == 0)
            {
                //Sparks and armor hit sound
            }
        }
    }

    public IEnumerator ShieldRecentlyBroken()
    {
        armorRecentlyBroken = true;
        yield return new WaitForSeconds(0.5f);
        armorRecentlyBroken = false;
    }
}
