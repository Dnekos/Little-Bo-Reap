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
            WorldState.instance.passiveValues.soulsCount += Random.Range(10, 17);
        }
    }

    //used to change soul count
    public void incrementSouls(int value)
    {
        WorldState.instance.passiveValues.soulsCount += value;

        //clamps player soul count to a positive number.
        if (WorldState.instance.passiveValues.soulsCount < 0)
        {
            WorldState.instance.passiveValues.soulsCount = 0;
        }
    }
}
