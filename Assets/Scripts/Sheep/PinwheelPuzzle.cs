using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinwheelPuzzle : MonoBehaviour
{
    [SerializeField] GameObject door;
    [SerializeField] List<Pinwheel> pinwheels;
    public bool isCheckingPinwheels = false;


    void Update()
    {
        //for now use this
        bool openDoor = true;

        for(int i = 0; i < pinwheels.Count; i++)
        {
            if (!pinwheels[i].isSpinning) openDoor = false;
        }

        //for now, destroy door. can have an animation or something more pretty later
        if(openDoor)
        {
            Destroy(door);
        }

    }
}
