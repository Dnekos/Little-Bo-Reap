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
}
