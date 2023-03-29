using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class descManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI descTextBox;
    [SerializeField] GameObject panel;

    [SerializeField] string desc;
    // Start is called before the first frame update

    public void MouseOver()
    {
        descTextBox.enabled = true;
        panel.GetComponent<Image>().enabled = true;
        descTextBox.text = desc;
    }

    public void MouseExit()
    {
        descTextBox.enabled = false;
        panel.GetComponent<Image>().enabled = false;
        descTextBox.text = "";
    }
}
