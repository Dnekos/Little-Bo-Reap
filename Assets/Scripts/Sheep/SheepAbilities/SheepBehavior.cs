using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class SheepBehavior : ScriptableObject
{
	abstract public void AbilityUpdate(PlayerSheepAI ps);
	abstract public void Begin(PlayerSheepAI ps, Vector3 targettedPos);
	abstract public void AbilityTriggerEnter(PlayerSheepAI ps, Collider other);
	abstract public void End(PlayerSheepAI ps, GameObject obj = null);
	abstract public bool IsRecallable(PlayerSheepAI ps);
}