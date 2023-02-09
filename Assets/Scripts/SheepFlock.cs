using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStampede", menuName = "ScriptableObjects/Stampede")]
public class SheepFlock : ScriptableObject
{
	public List<PlayerSheepAI> activeSheep;
	public int MaxSize;
	public GameObject SheepPrefab;
	public GameObject SheepProjectilePrefab;
	public Color UIColor;
	public Sprite sheepIcon;
	public ParticleSystem spellParticle;
	public ParticleSystem flockChangeParticle;
	public GameObject reticleSustainPrefab;
	public GameObject reticleConfirmPrefab;
}
