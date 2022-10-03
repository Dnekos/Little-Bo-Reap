using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class ButtonFunctionScript : MonoBehaviour
{
	public Button[] TreeButtons;
	public GameObject[] MenuCanvases;

	void Start () {
		
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
		for (int i = 0; i < TreeButtons.Length; i++)
		{
			Button btn = TreeButtons[i].GetComponent<Button>();
 			ColorBlock cb = btn.colors;
 			cb.normalColor = Color.white;
			cb.highlightedColor = Color.green;
			cb.pressedColor = Color.green;
			cb.selectedColor = Color.white;
			cb.disabledColor = Color.white;
 			btn.colors = cb;
		}
	}
}
