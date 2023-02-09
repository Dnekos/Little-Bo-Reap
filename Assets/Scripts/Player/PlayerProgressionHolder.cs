using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProgressionHolder : MonoBehaviour
{
    [Header("Soul Count")]
    public int soulsCount;
    public int bossSoulsCount;

    [Header("Active Abilities")]
    public string activeBuilderAbility;
    public string activeRamAbility;
    public string activeFluffyAbility;

    [Header("Builder Upgrades")]
    public int builderLaunchDam;
    public int builderCorruptMult;
    public int builderMaxStackHeight;

    [Header("Ram Upgrades")]
    public int ramDamageReduction;
    public int ramDamage;
    public int ramKnockback;

    [Header("Fluffy Upgrades")]
    public int fluffyHealth;
    public int fluffyKnockResist;
    public int fluffyVortexDuration;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //TODO remove after econ is figured out.
        if (Input.GetKeyDown(KeyCode.J))
        {
            soulsCount += Random.Range(10, 17);
        }
    }

    public void Upgrade(SheepTypes type, int index, int newValue)
    {
        switch (type)
        {
            case SheepTypes.BUILD:
                BuilderUpgrade(index, newValue);
                break;
            case SheepTypes.RAM:
                RamUpgrade(index, newValue);
                break;
            case SheepTypes.FLUFFY:
                FluffyUpgrade(index, newValue);
                break;
        }

            
    }

    void BuilderUpgrade(int index, int newValue)
    {
        switch (index)
        {
            case 0:
                builderLaunchDam = newValue;
                break;
            case 1:
                builderCorruptMult = newValue;
                break;
            case 2:
                builderMaxStackHeight = newValue;
                break;
        }
    }

    void RamUpgrade(int index, int newValue)
    {
        switch (index)
        {
            case 0:
                ramDamageReduction = newValue;
                break;
            case 1:
                ramDamage = newValue;
                break;
            case 2:
                ramKnockback = newValue;
                break;
        }
    }

    void FluffyUpgrade(int index, int newValue)
    {
        switch (index)
        {
            case 0:
                fluffyHealth = newValue;
                break;
            case 1:
                fluffyKnockResist = newValue;
                break;
            case 2:
                fluffyVortexDuration = newValue;
                break;
        }
    }

    //used to change soul count
    public void incrementSouls(int value)
    {
        soulsCount += value;
        //clamps player soul count to a positive number.
        if (soulsCount < 0)
        {
            soulsCount = 0;
        }
    }
}
