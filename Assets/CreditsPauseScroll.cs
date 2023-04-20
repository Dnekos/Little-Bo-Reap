using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsPauseScroll : MonoBehaviour
{
	[SerializeField] float MaxHeight = 300, Seconds = 30;
	float rate = 0;

	RectTransform rt;

	private void OnEnable()
	{
		if (rate <= 0)
			rate = MaxHeight / Seconds;

		if (rt == null)
			rt = GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, 0);
	}

	// Update is called once per frame
	void Update()
    {
		rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, rt.anchoredPosition.y + Time.unscaledDeltaTime * rate);
		if (rt.anchoredPosition.y >=MaxHeight)
			rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, 0);

	}
}
