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
		int flagindex = 1 << (((int)thisType + 1) * 3 + index + 1);

		Debug.Log("upgrading path:" + index);

		if (WorldState.instance.PersistentData.soulsCount >= costs[currentCostIndex] &&
			!WorldState.instance.PersistentData.boughtUpgrades.HasFlag((SaveData.Upgrades)flagindex))
		{
			//turn on light
			//TODO make this for all uptions
			ActivateUI(index);
			WorldState.instance.PersistentData.soulsCount -= costs[currentCostIndex];

			// TODO: make this not done every time, put it in start
			List<float[]> upgradesAllValues = new List<float[]>();
			upgradesAllValues.Add(upgrade1Values);
			upgradesAllValues.Add(upgrade2Values);
			upgradesAllValues.Add(upgrade3Values);

			// hard coding in 0, TODO: make them not arrays, unneeded now
			Upgrade(thisType, index, upgradesAllValues[index][0]);
			currentCostIndex++;
		}
		else
			Debug.Log("not enough money");
    }

	/// <summary>
	/// sets aesthetic change for a specific upgrade
	/// </summary>
	public void ActivateUI(int index)
	{
		for (int i = 0; i < upgrades[index].transform.childCount; i++)
		{
			Image childimage = upgrades[index].transform.GetChild(i).GetComponent<Image>();
			if (childimage != null && childimage.color != Color.green)
				childimage.color = Color.green;
		}
	}
    void AbilityChange(int index)
    {
        switch (thisType)
        {
            case 0:
                WorldState.instance.PersistentData.activeBuilderAbility = (ActiveUpgrade)(index);
                break;
            case 1:
                WorldState.instance.PersistentData.activeRamAbility = (ActiveUpgrade)(index);
                break;
            case 2:
                WorldState.instance.PersistentData.activeFluffyAbility = (ActiveUpgrade)(index);
                break;
        }
        
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
