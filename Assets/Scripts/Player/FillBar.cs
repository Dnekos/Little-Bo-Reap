using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FillBar : MonoBehaviour
{
	[SerializeField] Image FillingImage;
	[SerializeField] RectTransform GlowPos;
	[SerializeField] bool ReverseGlimmer = false;

	bool vertical;
	float original_y;
	float width;

	// Start is called before the first frame update
	void Start()
	{
		width = FillingImage.rectTransform.rect.xMax - FillingImage.rectTransform.rect.xMin;
		vertical = FillingImage.fillMethod == Image.FillMethod.Vertical;

		if (vertical)
		{
			width = FillingImage.rectTransform.rect.yMax - FillingImage.rectTransform.rect.yMin;
			original_y = GlowPos.anchoredPosition.x;
		}
		else
		{
			width = FillingImage.rectTransform.rect.xMax - FillingImage.rectTransform.rect.xMin;
			original_y = GlowPos.anchoredPosition.y;
		}

	}

	public void ChangeFill(float ratio)
	{

		float pos;
		if (ReverseGlimmer)
			pos = Mathf.Lerp(width, 0, ratio);
		else
			pos = Mathf.Lerp(0, width, ratio);

		GlowPos.anchoredPosition = vertical ? new Vector2(original_y, pos) :
												new Vector2(pos, original_y);
		GlowPos.gameObject.SetActive(ratio == 1);

		FillingImage.fillAmount = ratio;
	}

}
