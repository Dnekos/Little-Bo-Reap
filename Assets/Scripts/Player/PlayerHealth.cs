using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public class PlayerHealth : Damageable
{
	[SerializeField] FillBar healthBar;

	[Header("Hitstun"), Tooltip("Prevents movement Inputs and speed checking while active")]
	public bool HitStunned = false;
	[SerializeField]
	float hitstunTimer = 0.2f;

	[Header("Hurt Vignette")]
	[SerializeField] Volume hurtVignette;
	[SerializeField] float vignetteStrength = 1, vignetteTime = 0.2f;
	[SerializeField] float hurtCooldown = 0.1f;
	bool isHurt = false;


	[Header("Respawning")]
	[SerializeField]
	GameEvent RespawnEvent;

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

		// resume collisions
		rb.isKinematic = false;

		WorldState.instance.HUD.CloseDeathMenu();

		foreach (PlayerInput input in inputs)
			input.enabled = true;

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

	}

	protected override void OnDeath()
	{
		WorldState.instance.gameState = WorldState.State.Dead;

		FMODUnity.RuntimeManager.PlayOneShot(deathSound, transform.position);
		WorldState.instance.ChangeMusic(WorldState.instance.currentWorldTheme);

		// stop collisions
		rb.isKinematic = true;

		WorldState.instance.HUD.OpenDeathMenu();
			
		foreach (PlayerInput input in inputs)
			input.enabled = false;

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

	}
	#endregion
	
	public void Heal(float heal)
	{
		Health = Mathf.Min(MaxHealth, Health + heal);
		healthBar.ChangeFill(Health / MaxHealth);

	}

	public override void TakeDamage(Attack atk, Vector3 attackForward, float damageAmp = 1, float knockbackMultiplier = 1)
	{
		if(!isHurt)
        {
			StartCoroutine(HurtCooldown());

			StopCoroutine("HitVignette");
			StartCoroutine("HitVignette");

			Debug.Log("getting attacked lmao");

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
	}

	IEnumerator HurtCooldown()
    {
		isHurt = true;
		yield return new WaitForSeconds(hurtCooldown);
		isHurt = false;
    }

	IEnumerator HitVignette()
    {
		hurtVignette.gameObject.SetActive(true);
		hurtVignette.weight = vignetteStrength;
		float inverse_time = 1 / vignetteTime;

		for (; hurtVignette.weight > 0; hurtVignette.weight -= Time.deltaTime * inverse_time)
			yield return new WaitForEndOfFrame();

		hurtVignette.gameObject.SetActive(false);

    }

	IEnumerator HitstunTracker()
	{
		HitStunned = true;
		yield return new WaitForSeconds(hitstunTimer);
		HitStunned = false;
	}
}