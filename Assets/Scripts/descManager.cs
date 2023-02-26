using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class descManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI descTextBox;

    [SerializeField] string desc;
    // Start is called before the first frame update

    public void MouseOver()
    {
        descTextBox.gameObject.SetActive(true);
        descTextBox.text = desc;
    }

    public void MouseExit()
    {
        descTextBox.gameObject.SetActive(false);
        descTextBox.text = "";
    }
}
