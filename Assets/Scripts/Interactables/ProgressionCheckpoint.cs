using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionCheckpoint : Interactable
{
    public override void Interact()
    {
        //was having trouble referencing all the way down will clean this up later
        //TO DO: collapse this to a single line.
        WorldState inst = WorldState.instance;
        GameObject plyr = inst.player;
        Transform trm = plyr.transform;
        Transform parent = trm.parent;
        Transform child = parent.Find("Progression_Canvas");
        GameObject chldGO = child.gameObject;
        chldGO.SetActive(true);

        WorldState.instance.player.GetComponent<PlayerPauseMenu>().PauseGame();

        base.Interact();
    }
}
