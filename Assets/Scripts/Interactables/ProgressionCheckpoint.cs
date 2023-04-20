using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionCheckpoint : Interactable
{
	public override void Interact()
    {
		if (WorldState.instance.PersistentData.unlocks.HasFlag(SaveData.TutorialUnlocks.UpgradeMenu))
		{
			WorldState.instance.HUD.ToggleProgressionMenu(true);

			base.Interact();
		}
    }
	protected override void OnTriggerEnter(Collider other)
	{
		if (WorldState.instance.PersistentData.unlocks.HasFlag(SaveData.TutorialUnlocks.UpgradeMenu))
			base.OnTriggerEnter(other);
	}
}
