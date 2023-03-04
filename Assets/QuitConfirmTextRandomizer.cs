using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuitConfirmTextRandomizer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI keepPlayingText;
    [SerializeField] TextMeshProUGUI quitText;
    [SerializeField] List<string> keepPlayingLines;
    [SerializeField] List<string> quitLines;

    private void OnEnable()
    {
        int rand1 = Random.Range(0, keepPlayingLines.Count);

        keepPlayingText.text = keepPlayingLines[rand1];

        int rand2 = Random.Range(0, quitLines.Count);

        quitText.text = quitLines[rand2];
    }
}
