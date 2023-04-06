//REVIEW: Looks good!

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionCheckpoint : Interactable
{
	public override void Interact()
    {
		WorldState.instance.HUD.ToggleProgressionMenu(true);
		//WorldState.instance.player.GetComponent<PlayerPauseMenu>().PauseGame();

        base.Interact();
    }
}
