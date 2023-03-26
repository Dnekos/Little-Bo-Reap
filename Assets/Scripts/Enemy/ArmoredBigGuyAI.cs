using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ArmoredBigGuyAI : BigGuyAI
{
    [Header("Armor")]
	[SerializeField] GameObject ArmorBarCanvas;
	[SerializeField] Transform[] ArmorBars;
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
            base.TakeDamage(atk, attackForward, damageAmp, knockbackMultiplier);
        }
        if(atk.name == "Ram_Attack_Charge" && armorBroken == false)
        {
            ArmorBarCanvas.SetActive(false);
            armorBroken = true;
        }
    }
}
