using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressionParent : MonoBehaviour
{
    [SerializeField]
    Button[] upgrades;

    [SerializeField]
    Button[] abilities;

    [SerializeField]
    int[] costs = { 10, 25, 30, 40, 50, 80, 90, 124, 200 };

    [SerializeField]
    SheepTypes thisType;

    [SerializeField]
    int[] upgrade1Values = { 0, 10, 20, 50000 };

    [SerializeField]
    int[] upgrade2Values = { 0, 10, 20, 50000 };

    [SerializeField]
    int[] upgrade3Values = { 0, 10, 20, 50000 };

    int currentCostIndex;

    // Start is called before the first frame update
    void Start()
    {
        upgrades[0].onClick.AddListener(delegate { Upgrade(0); });
        upgrades[1].onClick.AddListener(delegate { Upgrade(1); });
        upgrades[2].onClick.AddListener(delegate { Upgrade(2); });
        
        abilities[0].onClick.AddListener(delegate { AbilityChange(0); });
        abilities[1].onClick.AddListener(delegate { AbilityChange(1); });
        abilities[2].onClick.AddListener(delegate { AbilityChange(2); });
    }

    void Upgrade(int index)
    {
        Debug.Log("upgrading path:" + index);
        PlayerProgressionHolder playerProgHold = WorldState.instance.player.GetComponent<PlayerProgressionHolder>();
        if (playerProgHold.soulsCount >= costs[currentCostIndex])
        {
            //turn on light
            //TODO make this for all uptions
            int upgradeNotch = -1;
            for (int i = 0; i < upgrades[index].transform.childCount; i++)
            {
                Transform notch = upgrades[index].transform.GetChild(i);
                if (notch.GetComponent<Image>().color != Color.green)
                {
                    notch.GetComponent<Image>().color = Color.green;
                    upgradeNotch = i;
                    break;
                }
            }
            if (upgradeNotch != -1)
            {
                playerProgHold.soulsCount -= costs[currentCostIndex];
                List<int[]> upgradesAllValues = new List<int[]>();
                upgradesAllValues.Add(upgrade1Values);
                upgradesAllValues.Add(upgrade2Values);
                upgradesAllValues.Add(upgrade3Values);
                int [] currentUpgradeValues = upgradesAllValues[index];
                playerProgHold.Upgrade(thisType, index, currentUpgradeValues[upgradeNotch]);
                currentCostIndex++;
            }
        }
        else
            Debug.Log("not enough money");
    }

    void AbilityChange(int index)
    {
        Debug.Log("turning ability to ability:" + index);
    }
}
