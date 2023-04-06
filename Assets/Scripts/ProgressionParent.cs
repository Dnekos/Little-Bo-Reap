using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressionParent : MonoBehaviour
{
    [SerializeField]
    Button[] upgrades;

    [SerializeField] Button[] abilities;

    [SerializeField] int[] costs = { 10, 25, 30, 40, 50, 80, 90, 124, 200 };

    [SerializeField] SheepTypes thisType;

    //Launch Dam, Charge DR, Fluffy Health
    [SerializeField] float[] upgrade1Values = { 10f, 20f, 50000f };

    //Black Sheep Chance, Ram Damage, Fluffy Knock Resist
    [SerializeField] float[] upgrade2Values = { 10f, 20f, 50000f };

    //Sheep Construct DR, Ram Knockback, Fluffy Vortex Duration
    [SerializeField] float[] upgrade3Values = { 10f, 20f, 50000f };

    int currentCostIndex;
    int abilitySoulCost = 1;
    bool altAbilityUnlocked = false;
    // Start is called before the first frame update
    void Start()
    {
        upgrades[0].onClick.AddListener(delegate { Upgrade(0); });
        upgrades[1].onClick.AddListener(delegate { Upgrade(1); });
        upgrades[2].onClick.AddListener(delegate { Upgrade(2); });
           
        abilities[0].onClick.AddListener(delegate { AbilityChange(0); });
        abilities[1].onClick.AddListener(delegate { AbilityChange(1); });
    }

    void Upgrade(int index)
    {
        Debug.Log("upgrading path:" + index);
        
        if (WorldState.instance.passiveValues.soulsCount >= costs[currentCostIndex])
        {
            //turn on light            
            int upgradeNotch = -1;
            Transform notch = upgrades[index].transform.parent.Find("notch");
            if (notch.GetComponent<Image>().color != Color.green)
            {
                notch.GetComponent<Image>().color = Color.green;
                upgradeNotch = index;             
            }
            if (upgradeNotch != -1)
            {
                WorldState.instance.passiveValues.soulsCount -= costs[currentCostIndex];
                List<float[]> upgradesAllValues = new List<float[]>();
                upgradesAllValues.Add(upgrade1Values);
                upgradesAllValues.Add(upgrade2Values);
                upgradesAllValues.Add(upgrade3Values);
                float [] currentUpgradeValues = upgradesAllValues[index];
                //in the event we move to multiple upgrades change this back to upgrade notch from 0
                Upgrade(thisType, index, currentUpgradeValues[0]);
                currentCostIndex++;
            }
        }
        else
            Debug.Log("not enough money");
    }

    void AbilityChange(int index)
    {
        switch (thisType)
        {
            case SheepTypes.BUILD:
                if(index == 0 || AbilityUnlock())
                {
                    WorldState.instance.passiveValues.activeBuilderAbility = "Ability " + index;
                    abilities[index].transform.parent.Find("notch").GetComponent<Image>().color = Color.green;
                    abilities[(index + 1) % 2].transform.parent.Find("notch").GetComponent<Image>().color = Color.white;
                }
                break;
            case SheepTypes.RAM:
                if (index == 0 || AbilityUnlock())
                {
                    WorldState.instance.passiveValues.activeRamAbility = "Ability " + index;
                    abilities[index].transform.parent.Find("notch").GetComponent<Image>().color = Color.green;
                    abilities[(index + 1) % 2].transform.parent.Find("notch").GetComponent<Image>().color = Color.white;
                }
                break;
            case SheepTypes.FLUFFY:
                if (index == 0 || AbilityUnlock())
                {
                    WorldState.instance.passiveValues.activeFluffyAbility = "Ability " + index;
                    abilities[index].transform.parent.Find("notch").GetComponent<Image>().color = Color.green;
                    abilities[(index + 1) % 2].transform.parent.Find("notch").GetComponent<Image>().color = Color.white;
                }
                break;
        }      
    }

    bool AbilityUnlock()
    {
        if(altAbilityUnlocked)
        {
            return true;
        }
        //if we have not unlocked already lets try to unlock.
        if(WorldState.instance.passiveValues.soulsCount >= abilitySoulCost)
        {
            WorldState.instance.passiveValues.soulsCount -= abilitySoulCost;
            altAbilityUnlocked = true;
            //add visual for unlocking alt ability here
            return true;
        }
        return false;
    }

    public void Upgrade(SheepTypes type, int index, float newValue)
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

    void BuilderUpgrade(int index, float newValue)
    {
        switch (index)
        {
            case 0:
                WorldState.instance.passiveValues.builderLaunchDam = newValue;
                Debug.Log("Set New Value: " + newValue.ToString());
                break;
            case 1:
                WorldState.instance.passiveValues.builderCorruptChance = newValue;
                break;
            case 2:
                WorldState.instance.passiveValues.builderConstructDR = newValue;
                break;
        }
    }

    void RamUpgrade(int index, float newValue)
    {
        switch (index)
        {
            case 0:
                WorldState.instance.passiveValues.ramChargeDR = newValue;
                break;
            case 1:
                WorldState.instance.passiveValues.ramDamage = newValue;
                break;
            case 2:
                WorldState.instance.passiveValues.ramKnockback = newValue;
                break;
        }
    }

    void FluffyUpgrade(int index, float newValue)
    {
        switch (index)
        {
            case 0:
                WorldState.instance.passiveValues.fluffyHealth = newValue;
                break;
            case 1:
                WorldState.instance.passiveValues.fluffyKnockResist = newValue;
                break;
            case 2:
                WorldState.instance.passiveValues.fluffyVortexDuration = newValue;
                break;
        }
    }

}
