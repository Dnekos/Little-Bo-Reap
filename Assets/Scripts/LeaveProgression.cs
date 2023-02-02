using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaveProgression : MonoBehaviour
{
    [SerializeField]
    Button exitButton;
    // Start is called before the first frame update
    void Start()
    {
        exitButton.onClick.AddListener(delegate { Exit(); });
    }

    private void Exit()
    {
        WorldState.instance.player.GetComponent<PlayerSheepAbilities>().DeleteAllSheep();
        WorldState.instance.player.GetComponent<PlayerPauseMenu>().PauseGame();
        transform.gameObject.SetActive(false);
    }
}
