using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillAllSheep : MonoBehaviour
{
    [SerializeField] SheepTypes sheepToSubtract;
    [SerializeField] int subtractAmount;
	[SerializeField, Min(0)] int MinimumSheep = 10;
    bool hasKilled;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && !hasKilled)
        {
            hasKilled = true;
			PlayerSheepAbilities player = other.GetComponent<PlayerSheepAbilities>();

			player.DeleteAllSheep();
			player.sheepFlocks[(int)sheepToSubtract].MaxSize = Mathf.Max(player.sheepFlocks[(int)sheepToSubtract].MaxSize - subtractAmount, MinimumSheep);
			player.UpdateFlockUI();

			// set save data
			WorldState.instance.PersistentData.totalBuilder = player.sheepFlocks[(int)SheepTypes.BUILD].MaxSize;
			WorldState.instance.PersistentData.totalRam = player.sheepFlocks[(int)SheepTypes.RAM].MaxSize;
			WorldState.instance.PersistentData.totalFluffy = player.sheepFlocks[(int)SheepTypes.FLUFFY].MaxSize;
        }
    }
}
