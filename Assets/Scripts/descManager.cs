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

	Image panelImage;
	RectTransform rt;
	Vector3[] corners;

	private void Start()
    {
        rt = GetComponent<RectTransform>();
        corners = new Vector3[4];
		panelImage = panel.GetComponent<Image>();
	}

	void OpenTooltip()
	{
		// if things aren't set, double check they are
		if (panelImage == null)
			Start();

		descTextBox.enabled = true;
		panelImage.enabled = true;

		rt.GetWorldCorners(corners);
		panel.transform.position = (corners[3] + corners[2]) * 0.5f;
		descTextBox.text = desc;
	}
	void CloseTooltip()
	{
		// if things aren't set, double check they are
		if (panelImage == null)
			Start();

		descTextBox.enabled = false;
		panelImage.enabled = false;
		descTextBox.text = "";
	}

	public void MouseOver()
    {
		OpenTooltip();
    }

    public void MouseExit()
    {
		CloseTooltip();
    }

    public void OnSelect()
    {
		OpenTooltip();
    }

    public void OnDeselect()
    {
		CloseTooltip();
    }
}
