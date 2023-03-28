using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProgressionHolder : MonoBehaviour
{
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
            WorldState.instance.PersistentData.soulsCount += Random.Range(10, 17);
            //SheepPassives.soulsCount += Random.Range(10, 17);
        }
    }

    //used to change soul count
    public void incrementSouls(int value)
    {
        WorldState.instance.PersistentData.soulsCount += value;
        WorldState.instance.HUD.UpdateSoulCount(WorldState.instance.PersistentData.soulsCount.ToString());
        //SheepPassives.soulsCount += value;

        //clamps player soul count to a positive number.

        if (WorldState.instance.PersistentData.soulsCount < 0)
        {
            WorldState.instance.PersistentData.soulsCount = 0;
        }
        
		/*
        if (SheepPassives.soulsCount < 0)
        {
            SheepPassives.soulsCount = 0;
        }
		*/
    }
}
