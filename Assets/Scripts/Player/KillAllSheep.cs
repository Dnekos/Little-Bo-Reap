using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillAllSheep : MonoBehaviour
{
    [SerializeField] SheepTypes sheepToSubtract;
    [SerializeField] int subtractAmount;
    bool hasKilled;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") && !hasKilled)
        {
            hasKilled = true;
            other.GetComponent<PlayerSheepAbilities>().DeleteAllSheep();
            other.GetComponent<PlayerSheepAbilities>().sheepFlocks[(int)sheepToSubtract].MaxSize -= subtractAmount;
            other.GetComponent<PlayerSheepAbilities>().UpdateFlockUI();
        }
    }
}
