using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossSoulCountUI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //TODO fix this to not be in update
        this.GetComponent<TextMeshProUGUI>().text = "Boss Soul Count: " + WorldState.instance.passiveValues.bossSoulsCount;
        //this.GetComponent<TextMeshProUGUI>().text = "Soul Count: " + SheepPassives.soulsCount;
    }
}
