//REVIEW: Overall, looks good, I would say just try to add more comments on stuff that might be confusing out of context
    //For example variables with vague names like 'index' or 'thisType'

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

    [SerializeField] int thisType;

    //Launch Dam, Charge DR, Fluffy Health
    [SerializeField] float[] upgrade1Values = { 10f, 20f, 50000f };

    //Black Sheep Chance, Ram Damage, Fluffy Knock Resist
    [SerializeField] float[] upgrade2Values = { 10f, 20f, 50000f };

    //Sheep Construct DR, Ram Knockback, Fluffy Vortex Duration
    [SerializeField] float[] upgrade3Values = { 10f, 20f, 50000f };

    int currentCostIndex;
    bool altAbilityUnlocked = false;
    int abilitySoulCost = 1;
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
        // we use bit shifting to get the correct bit at the flag we're looking for
        int exponent = index + (thisType * 3 + 1) - 1;
        int flagIndex = 1 << exponent;//(((int)thisType + 1) * 3 + index);//REVIEW: 'flagindex' --> 'flagIndex' to keep with camelCase standard


        if (WorldState.instance.PersistentData.soulsCount >= costs[0])
        {
            if (!WorldState.instance.PersistentData.boughtUpgrades.HasFlag((SaveData.Upgrades)flagIndex))
            {
                //turn on light
                //TODO make this for all uptions
                ActivateUI(index);
                WorldState.instance.PersistentData.soulsCount -= costs[0];
                WorldState.instance.HUD.UpdateSoulCount();

                // TODO: make this not done every time, put it in start
                List<float[]> upgradesAllValues = new List<float[]>();
                upgradesAllValues.Add(upgrade1Values);
                upgradesAllValues.Add(upgrade2Values);
                upgradesAllValues.Add(upgrade3Values);

                // hard coding in 0, TODO: make them not arrays, unneeded now
                Upgrade(thisType, index, upgradesAllValues[index][0]);
                currentCostIndex++;
            }
        }
        else
            Debug.Log("not enough money");
            //REVIEW: Maybe we can have a visual representation for the player to know they don't have enough souls
    }

	/// <summary>
	/// sets aesthetic change for a specific upgrade
	/// </summary>
	public void ActivateUI(int index)
	{
		for (int i = 0; i < upgrades[index].transform.parent.childCount; i++)
		{
			Image childimage = upgrades[index].transform.parent.Find("notch").GetComponent<Image>();
			if (childimage != null && childimage.color != Color.green)
				childimage.color = Color.green;
		}
	}
    void AbilityChange(int index)
    {
        switch (thisType)
        {
            case 0:
                if (index == 0 || AbilityUnlock())
                {
                    WorldState.instance.PersistentData.activeBuilderAbility = (ActiveUpgrade)(index);
                    abilities[index].transform.parent.Find("notch").GetComponent<Image>().color = Color.green;
                    abilities[(index + 1) % 2].transform.parent.Find("notch").GetComponent<Image>().color = Color.white;
                }
                break;
            case 1:
                if (index == 0 || AbilityUnlock())
                {
                    WorldState.instance.PersistentData.activeRamAbility = (ActiveUpgrade)(index);
                    abilities[index].transform.parent.Find("notch").GetComponent<Image>().color = Color.green;
                    abilities[(index + 1) % 2].transform.parent.Find("notch").GetComponent<Image>().color = Color.white;
                }
                break;
            case 2:
                if (index == 0 || AbilityUnlock())
                {
                    WorldState.instance.PersistentData.activeFluffyAbility = (ActiveUpgrade)(index);
                    abilities[index].transform.parent.Find("notch").GetComponent<Image>().color = Color.green;
                    abilities[(index + 1) % 2].transform.parent.Find("notch").GetComponent<Image>().color = Color.white;
                }
                break;
        }
    }
    bool AbilityUnlock()
    {
        if (altAbilityUnlocked)
        {
            return true;
        }
        //if we have not unlocked already lets try to unlock.
        if (WorldState.instance.PersistentData.soulsCount >= abilitySoulCost)
        {
            WorldState.instance.PersistentData.soulsCount -= abilitySoulCost;
            altAbilityUnlocked = true;
            //add visual for unlocking alt ability here
            return true;
        }
        return false;
    }
    public void CheckLoadedUpgrades()
	{
		List<float[]> upgradesAllValues = new List<float[]>();
		upgradesAllValues.Add(upgrade1Values);
		upgradesAllValues.Add(upgrade2Values);
		upgradesAllValues.Add(upgrade3Values);

		for (int i = 0; i < upgrades.Length; i++)
		{
			// we use bit shifting to get the correct bit at the flag we're looking for
			int flagindex = 1 << (thisType * 3 + i);
			if (WorldState.instance.PersistentData.boughtUpgrades.HasFlag((SaveData.Upgrades)flagindex))
			{
				ActivateUI(i);
				Upgrade(thisType, i, upgradesAllValues[i][0]);
			}
		}

	}

	public void Upgrade(int type, int index, float newValue)
    {
        switch (type)
        {
            case 0:
                BuilderUpgrade(index, newValue);
                break;
            case 1:
                RamUpgrade(index, newValue);
                break;
            case 2:
                FluffyUpgrade(index, newValue);
                break;
        }
    }

    void BuilderUpgrade(int index, float newValue)
    {
		Debug.Log(index);
        switch (index)
        {
            case 0:
                WorldState.instance.passiveValues.builderLaunchDam = newValue;
				WorldState.instance.PersistentData.boughtUpgrades |= SaveData.Upgrades.BuilderLaunchDam;
                break;
            case 1:
                WorldState.instance.passiveValues.builderCorruptChance = newValue;
				WorldState.instance.PersistentData.boughtUpgrades |= SaveData.Upgrades.BuilderCorruptChance;
				break;
            case 2:
                WorldState.instance.passiveValues.builderConstructDR = newValue;
				WorldState.instance.PersistentData.boughtUpgrades |= SaveData.Upgrades.BuilderConstructDR;
				break;
        }
    }

    void RamUpgrade(int index, float newValue)
    {
        switch (index)
        {
            case 0:
                WorldState.instance.passiveValues.ramChargeDR = newValue;
				WorldState.instance.PersistentData.boughtUpgrades |= SaveData.Upgrades.RamChargeDR;
				break;
            case 1:
                WorldState.instance.passiveValues.ramDamage = newValue;
				WorldState.instance.PersistentData.boughtUpgrades |= SaveData.Upgrades.RamDamage;
				break;
            case 2:
                WorldState.instance.passiveValues.ramKnockback = newValue;
				WorldState.instance.PersistentData.boughtUpgrades |= SaveData.Upgrades.RamKnockback;
				break;
        }
    }

    void FluffyUpgrade(int index, float newValue)
    {
        switch (index)
        {
            case 0:
                WorldState.instance.passiveValues.fluffyHealth = newValue;
				WorldState.instance.PersistentData.boughtUpgrades |= SaveData.Upgrades.FluffyHealth;
				break;
            case 1:
                WorldState.instance.passiveValues.fluffyKnockResist = newValue;
				WorldState.instance.PersistentData.boughtUpgrades |= SaveData.Upgrades.FluffyKnockResist;
				break;
            case 2:
                WorldState.instance.passiveValues.fluffyVortexDuration = newValue;
				WorldState.instance.PersistentData.boughtUpgrades |= SaveData.Upgrades.FluffyVortexDuration;
				break;
        }
    }

}
