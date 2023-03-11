using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SymbolFill : MonoBehaviour
{
    [SerializeField] private Image topSym;
    [SerializeField] private AbilityIcon icon;

    // Start is called before the first frame update
    void Start()
    {
        icon.fillableChanged += ChangeFillOnSym;
    }

    void ChangeFillOnSym(float change)
	{
        topSym.fillAmount = change;
	}
}
