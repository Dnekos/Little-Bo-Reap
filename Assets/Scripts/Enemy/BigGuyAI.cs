using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BigGuyAI : EnemyAI
{
	[Header("HealthBar")]
	[SerializeField] public GameObject HealthBarCanvas;
	[SerializeField] public Transform[] HPBars;

	[Header("Shockwave")]
	[SerializeField] Transform ShockwaveSpawnPoint;

	// Start is called before the first frame update
	override protected void Start()
    {
		base.Start();
		
		HealthBarCanvas.SetActive(false);
	}

	void FixedUpdate()
    {
		if (currentEnemyState == EnemyStates.HITSTUN || executeTrigger.active == true)
        {
			GetAnimator().Play("BigGuy_Stun");
        }

	}		

	// for animation trigger
	public void SpawnShockwave()
	{
		if (activeAttack != null)
			activeAttack.SpawnObject(ShockwaveSpawnPoint.position, Quaternion.identity);
	}	


	#region Healthbar
	protected override void Update()
	{
		base.Update();

		// if healthbar is active, billboard it
		if (HealthBarCanvas.activeSelf && Health != MaxHealth)
			HealthBarCanvas.transform.LookAt(Camera.main.transform);

		if (currentEnemyState == EnemyStates.EXECUTABLE)
			HealthBarCanvas.SetActive(false);
	}

	public override void TakeDamage(Attack atk, Vector3 attackForward, float damageAmp = 1, float knockbackMultiplier = 1)
	{
		base.TakeDamage(atk, attackForward);

		// when taking damage, open healthbar
		if (Health != MaxHealth && Health > executionHealthThreshhold)
		{
			HealthBarCanvas.SetActive(true);
			float healthbarScale = 1 - (Health / MaxHealth);
			HPBars[0].localScale = new Vector3(healthbarScale, 1, 1);
			HPBars[1].localScale = new Vector3(healthbarScale * -1, 1, 1);
		}
		else
			HealthBarCanvas.SetActive(false);

	}
	#endregion
}