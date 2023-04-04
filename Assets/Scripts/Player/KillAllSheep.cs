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
        }
    }
}
