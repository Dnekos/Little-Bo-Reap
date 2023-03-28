using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessBattlePlayerDebug : MonoBehaviour
{
    public PlayerSheepAbilities sheepAbilities;
    [SerializeField] int builderSize;
    [SerializeField] int ramSize;
    [SerializeField] int fluffySize;

    //I couldn't figure out what dumbass script overwrites setting flock size in editor so im doing this. 
    private void Start()
    {
        Invoke("SetFlock", 0.1f);
    }

    void SetFlock()
    {
        sheepAbilities.sheepFlocks[0].MaxSize = builderSize;
        sheepAbilities.sheepFlocks[1].MaxSize = ramSize;
        sheepAbilities.sheepFlocks[2].MaxSize = fluffySize;
        sheepAbilities.UpdateFlockUI();

        WorldState.instance.PersistentData.soulsCount += 999;
        WorldState.instance.PersistentData.bossSoulsCount += 999;
    }
}
