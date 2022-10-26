using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerHealth : Damageable
{
	[SerializeField] FillBar healthBar;

	[Header("Hitstun"), Tooltip("Prevents movement Inputs and speed checking while active")]
	public bool HitStunned = false;
	[SerializeField]
	float hitstunTimer = 0.2f;


	[Header("Respawning")]
	[SerializeField]
	GameEvent RespawnEvent;
	[SerializeField]
	GameObject HUD, DeathUI;

	[SerializeField]
	PlayerInput[] inputs;
	PlayerMovement playermove;

	// Start is called before the first frame update
	protected override void Start()
	{
		base.Start();
		playermove = GetComponent<PlayerMovement>();
		healthBar.ChangeFill(Health / MaxHealth);

		RespawnEvent.listener.AddListener(delegate { ResetHealth(); });
	}

	#region Respawn UI buttons
	public void RestartLevel()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

	}
	public void Respawn()
	{
		RespawnEvent.listener.Invoke();
	}
	#endregion

	#region Death and Respawning

	void ResetHealth()
	{
		Health = MaxHealth;
		healthBar.ChangeFill(1);

		HUD.SetActive(true);
		DeathUI.SetActive(false);
		foreach (PlayerInput input in inputs)
			input.enabled = true;

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

	}

	protected override void OnDeath()
	{
		WorldState.instance.Dead = true;

		HUD.SetActive(false);
		DeathUI.SetActive(true);
		foreach (PlayerInput input in inputs)
			input.enabled = false;

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

	}
	#endregion

	public override void TakeDamage(Attack atk, Vector3 attackForward)
	{
		// stop moving, to hopefully prevent too wacky knockback
		rb.AddForce(-rb.velocity, ForceMode.VelocityChange);

		base.TakeDamage(atk, attackForward);
		healthBar.ChangeFill(Health / MaxHealth);

		if (atk.DealsHitstun)
		{
			StopCoroutine("HitstunTracker");
			StartCoroutine("HitstunTracker");
		}
	}
	IEnumerator HitstunTracker()
	{
		HitStunned = true;
		yield return new WaitForSeconds(hitstunTimer);
		HitStunned = false;
	}
}