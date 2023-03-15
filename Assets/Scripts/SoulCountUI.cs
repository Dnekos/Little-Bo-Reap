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
        
    }

    // Update is called once per frame
    void Update()
    {
        //TODO fix this to not be in update
        this.GetComponent<TextMeshProUGUI>().text = "Soul Count: " + WorldState.instance.PersistentData.soulsCount;
        //this.GetComponent<TextMeshProUGUI>().text = "Soul Count: " + SheepPassives.soulsCount;
    }
}
