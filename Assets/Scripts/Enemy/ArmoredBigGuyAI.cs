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
    private bool armorBroken = false;

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
        if(armorBroken == true)
        {
            base.TakeDamage(atk, attackForward);
            //REVIEW: I would use >= for the health execution if check
            if (Health != MaxHealth || Health > executionHealthThreshhold)
            {
                HealthBarCanvas.SetActive(true);
                float healthbarScale = (Health / MaxHealth);
                HPBars[0].localScale = new Vector3(healthbarScale, 1, 1);
                HPBars[1].localScale = new Vector3(healthbarScale * -1, 1, 1);
            }
            else
                HealthBarCanvas.SetActive(false);
        }
        if (atk.name == "Ram_Attack_Charge" && armorBroken == false)
        {
            ArmorBarCanvas.SetActive(false);
            HealthBarCanvas.SetActive(true);
            armorObject.SetActive(false);
            destroyParticles.Play(true);
            armorBroken = true;
        }
    }
}
