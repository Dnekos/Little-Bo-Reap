//REVIEW: Overall, looks good, I would say just try to add more comments on stuff that might be confusing out of context
    //For example variables with vague names like 'index' or 'thisType'

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressionParent : MonoBehaviour
{
    [SerializeField]
	ButtonClickToggle[] upgrades;

	[SerializeField] ButtonClickToggle abilities;

    [SerializeField] int[] costs = { 10, 25, 30, 40, 50, 80, 90, 124, 200 };

    [SerializeField] int thisType;

    //Launch Dam, Charge DR, Fluffy Health
    [SerializeField] float[] upgrade1Values = { 10f, 20f, 50000f };

    //Black Sheep Chance, Ram Damage, Fluffy Knock Resist
    [SerializeField] float[] upgrade2Values = { 10f, 20f, 50000f };

    //Sheep Construct DR, Ram Knockback, Fluffy Vortex Duration
    [SerializeField] float[] upgrade3Values = { 10f, 20f, 50000f };

	// ingame used things
    int currentCostIndex;
    bool altAbilityUnlocked = false;
    int abilitySoulCost = 1;
	SaveData.Upgrades abilityFlag = SaveData.Upgrades.None;
	List<float[]> upgradesAllValues;
	[Header("Sound")]
	[SerializeField] FMODUnity.EventReference activatedSFX;
	[SerializeField] FMODUnity.EventReference deactivatedSFX;

	// Start is called before the first frame update
	void Start()
    {
		Initialize();
		upgrades[0].button.onClick.AddListener(delegate { Upgrade(0); });
        upgrades[1].button.onClick.AddListener(delegate { Upgrade(1); });
        upgrades[2].button.onClick.AddListener(delegate { Upgrade(2); });
        
        abilities.button.onClick.AddListener(delegate { AbilityChange(); });
        //abilities[1].onClick.AddListener(delegate { AbilityChange(1); });
    }

	void Initialize()
	{
		abilityFlag = (SaveData.Upgrades)(1 << thisType + 9);
		upgradesAllValues = new List<float[]>();
		upgradesAllValues.Add(upgrade1Values);
		upgradesAllValues.Add(upgrade2Values);
		upgradesAllValues.Add(upgrade3Values);
	}

	void Upgrade(int index)
	{
		// we use bit shifting to get the correct bit at the flag we're looking for
		int exponent = index + (thisType * 3);
		int flagIndex = 1 << exponent; //REVIEW: 'flagindex' --> 'flagIndex' to keep with camelCase standard

		if (!WorldState.instance.PersistentData.boughtUpgrades.HasFlag((SaveData.Upgrades)flagIndex))
		{
			if (WorldState.instance.PersistentData.soulsCount >= costs[0])
			{
				WorldState.instance.PersistentData.soulsCount -= costs[0];
				WorldState.instance.HUD.UpdateSoulCount();
			}
			else
			{
				Debug.Log("not enough money");
				return;
			}
		}

		// hard coding in 0, TODO: make them not arrays, unneeded now
		Upgrade(thisType, index, upgradesAllValues[index][0]);

		ActivateUpgradeUI(index, flagIndex);

		currentCostIndex++;


		//REVIEW: Maybe we can have a visual representation for the player to know they don't have enough souls
	}

	/// <summary>
	/// sets aesthetic change for a specific upgrade
	/// </summary>
	public void ActivateUpgradeUI(int index, int flagIndex)
	{
		if (WorldState.instance.PersistentData.activeUpgrades.HasFlag((SaveData.Upgrades)flagIndex))
		{
			upgrades[index].SetState(ButtonState.Enabled);
			FMODUnity.RuntimeManager.PlayOneShot(activatedSFX);
		}
		else if (WorldState.instance.PersistentData.boughtUpgrades.HasFlag((SaveData.Upgrades)flagIndex))
		{
			upgrades[index].SetState(ButtonState.Disabled);
			FMODUnity.RuntimeManager.PlayOneShot(deactivatedSFX);
		}
		else
			upgrades[index].SetState(ButtonState.Locked);

	}
	public void ActivateAbilityUI()
	{
		if (WorldState.instance.PersistentData.activeUpgrades.HasFlag(abilityFlag))
		{
			abilities.SetState(ButtonState.Enabled);
			FMODUnity.RuntimeManager.PlayOneShot(activatedSFX);
		}
		else if (WorldState.instance.PersistentData.boughtUpgrades.HasFlag(abilityFlag))
		{
			abilities.SetState(ButtonState.Disabled);
			FMODUnity.RuntimeManager.PlayOneShot(deactivatedSFX);
		}
		else
			abilities.SetState(ButtonState.Locked);

	}
	void AbilityChange()
    {
		if (UnlockAbility())
		{
			WorldState.instance.PersistentData.boughtUpgrades |= abilityFlag;
			WorldState.instance.PersistentData.activeUpgrades ^= abilityFlag;

			ActivateAbilityUI();
		}
    }
	/// <summary>
	/// subtracts soulcount from worldstate and returns if ability is already unlocked or was purchased
	/// </summary>
    bool UnlockAbility()
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
		if (abilityFlag == SaveData.Upgrades.None)
			Initialize();

		for (int i = 0; i < upgrades.Length; i++)
		{
			// we use bit shifting to get the correct bit at the flag we're looking for
			SaveData.Upgrades flagindex = (SaveData.Upgrades)(1 << i + (thisType * 3));
			if (WorldState.instance.PersistentData.activeUpgrades.HasFlag(flagindex))
			{
				Upgrade(thisType, i, upgradesAllValues[i][0]);
				upgrades[i].SetState(ButtonState.Enabled);
			}
			else if (WorldState.instance.PersistentData.boughtUpgrades.HasFlag(flagindex))
			{
				upgrades[i].SetState(ButtonState.Disabled);
				Debug.Log("disabled " + flagindex);

			}
			else
			{ 
				upgrades[i].SetState(ButtonState.Locked);
				Debug.Log("locked "+flagindex);
	}
		}

		// active abilities
		altAbilityUnlocked = WorldState.instance.PersistentData.boughtUpgrades.HasFlag(abilityFlag);
		ActivateAbilityUI();
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
				WorldState.instance.PersistentData.boughtUpgrades |= SaveData.Upgrades.BuilderLaunchDam;
				WorldState.instance.PersistentData.activeUpgrades ^= SaveData.Upgrades.BuilderLaunchDam; // toggle it being active

				// set value
				if (WorldState.instance.PersistentData.activeUpgrades.HasFlag(SaveData.Upgrades.BuilderLaunchDam))
					WorldState.instance.passiveValues.builderLaunchDam = newValue;
				else
					WorldState.instance.passiveValues.builderLaunchDam = 0;

				break;
            case 1:
				WorldState.instance.PersistentData.boughtUpgrades |= SaveData.Upgrades.BuilderCorruptChance;
				WorldState.instance.PersistentData.activeUpgrades ^= SaveData.Upgrades.BuilderCorruptChance; // toggle it being active

				// set value
				if (WorldState.instance.PersistentData.activeUpgrades.HasFlag(SaveData.Upgrades.BuilderCorruptChance))
					WorldState.instance.passiveValues.builderCorruptChance = newValue;
				else
					WorldState.instance.passiveValues.builderCorruptChance = 0;

				break;
            case 2:
				WorldState.instance.PersistentData.boughtUpgrades |= SaveData.Upgrades.BuilderConstructDR;
				WorldState.instance.PersistentData.activeUpgrades ^= SaveData.Upgrades.BuilderConstructDR; // toggle it being active

				// set value
				if (WorldState.instance.PersistentData.activeUpgrades.HasFlag(SaveData.Upgrades.BuilderConstructDR))
					WorldState.instance.passiveValues.builderConstructDR = newValue;
				else
					WorldState.instance.passiveValues.builderConstructDR = 0;

				break;
        }
    }

    void RamUpgrade(int index, float newValue)
    {
        switch (index)
        {
            case 0:
				WorldState.instance.PersistentData.boughtUpgrades |= SaveData.Upgrades.RamChargeDR;
				WorldState.instance.PersistentData.activeUpgrades ^= SaveData.Upgrades.RamChargeDR; // toggle it being active

				// set value
				if (WorldState.instance.PersistentData.activeUpgrades.HasFlag(SaveData.Upgrades.RamChargeDR))
					WorldState.instance.passiveValues.ramChargeDR = newValue;
				else
					WorldState.instance.passiveValues.ramChargeDR = 0;

				break;
            case 1:
				WorldState.instance.PersistentData.boughtUpgrades |= SaveData.Upgrades.RamDamage;
				WorldState.instance.PersistentData.activeUpgrades ^= SaveData.Upgrades.RamDamage; // toggle it being active

				// set value
				if (WorldState.instance.PersistentData.activeUpgrades.HasFlag(SaveData.Upgrades.RamDamage))
					WorldState.instance.passiveValues.ramDamage = newValue;
				else
					WorldState.instance.passiveValues.ramDamage = 0;

				break;
            case 2:
				WorldState.instance.PersistentData.boughtUpgrades |= SaveData.Upgrades.RamKnockback;
				WorldState.instance.PersistentData.activeUpgrades ^= SaveData.Upgrades.RamKnockback; // toggle it being active

				// set value
				if (WorldState.instance.PersistentData.activeUpgrades.HasFlag(SaveData.Upgrades.RamKnockback))
					WorldState.instance.passiveValues.ramKnockback = newValue;
				else
					WorldState.instance.passiveValues.ramKnockback = 0;

				break;
        }
    }

    void FluffyUpgrade(int index, float newValue)
    {
        switch (index)
        {
            case 0:
				WorldState.instance.PersistentData.boughtUpgrades |= SaveData.Upgrades.FluffyHealth;
				WorldState.instance.PersistentData.activeUpgrades ^= SaveData.Upgrades.FluffyHealth; // toggle it being active

				// set value
				if (WorldState.instance.PersistentData.activeUpgrades.HasFlag(SaveData.Upgrades.FluffyHealth))
					WorldState.instance.passiveValues.fluffyHealth = newValue;
				else
					WorldState.instance.passiveValues.fluffyHealth = 0;

				break;
            case 1:
				WorldState.instance.PersistentData.boughtUpgrades |= SaveData.Upgrades.FluffyKnockResist;
				WorldState.instance.PersistentData.activeUpgrades ^= SaveData.Upgrades.FluffyKnockResist; // toggle it being active

				// set value
				if (WorldState.instance.PersistentData.activeUpgrades.HasFlag(SaveData.Upgrades.FluffyKnockResist))
					WorldState.instance.passiveValues.fluffyKnockResist = newValue;
				else
					WorldState.instance.passiveValues.fluffyKnockResist = 0;

				break;
            case 2:
				WorldState.instance.PersistentData.boughtUpgrades |= SaveData.Upgrades.FluffyVortexDuration;
				WorldState.instance.PersistentData.activeUpgrades ^= SaveData.Upgrades.FluffyVortexDuration; // toggle it being active

				// set value
				if (WorldState.instance.PersistentData.activeUpgrades.HasFlag(SaveData.Upgrades.FluffyVortexDuration))
					WorldState.instance.passiveValues.fluffyVortexDuration = newValue;
				else
					WorldState.instance.passiveValues.fluffyVortexDuration = 0;

				break;
        }
    }

}
