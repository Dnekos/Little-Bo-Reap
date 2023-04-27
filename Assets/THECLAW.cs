using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class THECLAW : MonoBehaviour
{

    [SerializeField] Image image;
    [SerializeField] TextMeshProUGUI theText;
    [SerializeField] float alphaScale;
    [SerializeField] DialogBox DB;
    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        image.color = new Color(image.color.r, image.color.g, image.color.b, (image.color.a + alphaScale));
        theText.color = new Color(theText.color.r, theText.color.g, theText.color.b, (theText.color.a + alphaScale));

        if (image.color.a >= 1) //once THE CLAW is fully visible, allow the game to end
        {
            DB.SetClawOpen(true);
        }
    }
}
