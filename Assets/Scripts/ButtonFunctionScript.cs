using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class ButtonFunctionScript : MonoBehaviour
{
	public Button[] TreeOneButtons;
	public Button[] TreeTwoButtons;
	public Button[] TreeThreeButtons;
	public GameObject[] MenuCanvases;

	void Start () 
	{
		
	}

	public void LoadTree(int treeToLoad) {
        Debug.Log ("Load Tree: " + treeToLoad.ToString());

		//deactivates all other "Menus"
		for (int i = 0; i < MenuCanvases.Length; i++)
		{
			MenuCanvases[i].SetActive(false);
		}

		MenuCanvases[treeToLoad].SetActive(true);
	}

	public void MarkSkill(Button b) {
		
		Debug.Log("Set Button Color");
		
		Button btn = b.GetComponent<Button>();
 		ColorBlock cb = btn.colors;
 		cb.normalColor = Color.red;
		cb.highlightedColor = Color.red;
		cb.pressedColor = Color.red;
		cb.selectedColor = Color.red;
		cb.disabledColor = Color.red;
 		btn.colors = cb;
		
	}
	public void ResetTree(int treeToReset) {
		
		//resets whichever buttons are put in the tree.
		switch (treeToReset)
		{
			case 0:
			for (int i = 0; i < TreeOneButtons.Length; i++)
			{
				Button btn = TreeOneButtons[i].GetComponent<Button>();
 				ColorBlock cb = btn.colors;
 				cb.normalColor = Color.white;
				cb.highlightedColor = Color.green;
				cb.pressedColor = Color.green;
				cb.selectedColor = Color.white;
				cb.disabledColor = Color.white;
 				btn.colors = cb;
			}
			break;
			case 1:
			for (int i = 0; i < TreeTwoButtons.Length; i++)
			{
				Button btn = TreeTwoButtons[i].GetComponent<Button>();
 				ColorBlock cb = btn.colors;
 				cb.normalColor = Color.white;
				cb.highlightedColor = Color.green;
				cb.pressedColor = Color.green;
				cb.selectedColor = Color.white;
				cb.disabledColor = Color.white;
 				btn.colors = cb;
			}
			break;
			//reset 
			case 2:
			for (int i = 0; i < TreeThreeButtons.Length; i++)
			{
				Button btn = TreeThreeButtons[i].GetComponent<Button>();
 				ColorBlock cb = btn.colors;
 				cb.normalColor = Color.white;
				cb.highlightedColor = Color.green;
				cb.pressedColor = Color.green;
				cb.selectedColor = Color.white;
				cb.disabledColor = Color.white;
 				btn.colors = cb;
			}
			break;
			default:
			Debug.Log("Reset Button Out Of Bounds");
			break;
		}
		
	}
}
