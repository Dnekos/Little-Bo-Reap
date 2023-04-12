//REVIEW: This looks good, but I would say maybe change the name of the 'panel' variable,
    //because I wasn't 100% sure what that would represent in engine

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class descManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI descTextBox;
    [SerializeField] GameObject panel;
    [SerializeField] Vector3 panelOffset;

    [SerializeField] string desc;
    // Start is called before the first frame update

    public void MouseOver()
    {
        descTextBox.enabled = true;
        panel.GetComponent<Image>().enabled = true;
        panel.transform.position = transform.position + panelOffset;
        descTextBox.text = desc;
    }

    public void MouseExit()
    {
        descTextBox.enabled = false;
        panel.GetComponent<Image>().enabled = false;
        descTextBox.text = "";
    }

    public void OnSelect()
    {
        descTextBox.enabled = true;
        panel.GetComponent<Image>().enabled = true;
        panel.transform.position = transform.position + panelOffset;
        descTextBox.text = desc;
    }

    public void OnDeselect()
    {
        descTextBox.enabled = false;
        panel.GetComponent<Image>().enabled = false;
        descTextBox.text = "";
    }
}
