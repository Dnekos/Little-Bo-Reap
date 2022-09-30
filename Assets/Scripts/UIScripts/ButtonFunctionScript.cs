using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class ButtonFunctionScript : MonoBehaviour
{
	public Button yourButton;

	void Start () {
		Button btn = yourButton.GetComponent<Button>();
		btn.onClick.AddListener(LoadTree);
	}

	void LoadTree() {
        Debug.Log ("Load Specific Tree: " + treeName);
	}
}
