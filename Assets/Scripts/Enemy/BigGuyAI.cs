using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class BigGuyAI : EnemyAI
{
	[Header("HealthBar")]
	[SerializeField] public GameObject HealthBarCanvas;
	[SerializeField] public Image HPBar;

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
		GetAnimator().SetBool("isStunned", currentEnemyState == EnemyStates.HITSTUN || executeTrigger.activeInHierarchy == true);

		//apply gravity if falling
		if (currentEnemyState == EnemyStates.HITSTUN || currentEnemyState == EnemyStates.EXECUTABLE)
			rb.AddForce(Vector3.down * fallRate, ForceMode.Impulse);//was previously accelerationd
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
		// stop enemies when they get stunned
		if (atk.DealsHitstun)
		{
			// make sure agent is NOT moving if we want them to stop
			NavMeshAgent agent = GetAgent();
			if (agent.enabled)
			{
				agent.speed = 0;
				agent.acceleration = 0;
				agent.isStopped = true;
				agent.velocity = Vector3.zero;
			}
		}

		base.TakeDamage(atk, attackForward);

		// when taking damage, open healthbar
		if (Health != MaxHealth && Health > executionHealthThreshhold)
		{
			HealthBarCanvas.SetActive(true);
			float healthbarScale = (Health / MaxHealth);
			HPBar.fillAmount = healthbarScale;
			//HPBars[1].localScale = new Vector3(healthbarScale * -1, 1, 1);
		}
		else
			HealthBarCanvas.SetActive(false);

	}
	#endregion
}