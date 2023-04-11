using UnityEngine;
using UnityEngine.UI;

public class ActiveButton : MonoBehaviour
{
	[SerializeField] private HUDManager m_hudManager;
    [SerializeField] private Color activeColor;
	[SerializeField] private Color inactiveColor;
	[SerializeField] private Image imageButton;
	[SerializeField] private Image imageBack;

	private AbilityIcon icon;

    void Start()
    {
		m_hudManager.activePanelChange += HUD_activePanelChange;
    }

	private void HUD_activePanelChange(GameObject obj)
	{
		//subscribe to new event
		if(icon != null)
		icon.fillableChanged -= activeFillable;

		AbilityIcon newIcon = obj.GetComponentInChildren<AbilityIcon>();
		newIcon.fillableChanged += activeFillable;

		//if (newIcon.Image.fillAmount == 0)
		//{
		//	newIcon.UIIsEnabled();
		//}

		icon = newIcon;
	}

    private void activeFillable(float fill)
	{
		//lerp between the two colors based on the current fill
		Color currentColor = Color.Lerp(inactiveColor, activeColor, fill);

		imageButton.color = currentColor;
		imageBack.color = currentColor;
	}
}
