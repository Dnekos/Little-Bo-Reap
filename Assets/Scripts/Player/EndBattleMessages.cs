using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EndBattleMessages : MonoBehaviour
{
    [SerializeField] List<string> endBattleMessages;
    [SerializeField] TextMeshProUGUI endBattleText;

    private void OnEnable()
    {
        int rand = Random.Range(0, endBattleMessages.Count);

        endBattleText.text = endBattleMessages[rand];
    }
}
