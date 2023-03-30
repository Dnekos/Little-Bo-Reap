using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SoulCountUI : MonoBehaviour
{
   
    // Start is called before the first frame update
    void Start()
    {
        
    }//REVIEW: we can delete this to clean the script up a little

    // Update is called once per frame
    void Update()
    {
        //TODO fix this to not be in update
        this.GetComponent<TextMeshProUGUI>().text = "Soul Count: " + WorldState.instance.PersistentData.soulsCount;
        //this.GetComponent<TextMeshProUGUI>().text = "Soul Count: " + SheepPassives.soulsCount;
    }
    //REVIEW: I see we are planning on getting this out of a update loop.
        //Maybe only change it when the "WorldState.instance.PersistentData.soulsCount" number gets changed, wherever that may be
}
