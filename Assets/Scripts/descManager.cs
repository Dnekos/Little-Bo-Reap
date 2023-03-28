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

    [SerializeField] string desc;
    // Start is called before the first frame update

    public void MouseOver()
    {
        descTextBox.gameObject.SetActive(true);
        panel.SetActive(true);
        descTextBox.text = desc;
    }

    public void MouseExit()
    {
        descTextBox.gameObject.SetActive(false);
        panel.SetActive(false);
        descTextBox.text = "";
    }
}
