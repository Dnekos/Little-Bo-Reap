using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGroundPound : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] FMODUnity.EventReference explodeSound;
    [SerializeField] string heavyAirAnimation;

    [Header("Timing / Conditions")]
    [SerializeField] float coolDown = 3f;
    [SerializeField] float timeTillSlamDown = 0.25f;
    [SerializeField] float minHeight;
    [SerializeField, Tooltip("how far the raycast goes")] float maxHeight = 100;
    [SerializeField] LayerMask groundMask;

    [Header("Damage")]
    [SerializeField] float baseGroundDamage = 25f;
    [SerializeField] float baseGroundDamageBlack = 50f;
    Vector3 startFallPos;

    [Header("Physics")]
    [SerializeField] float airUpForce;
    [SerializeField] float airDownForce;
    [SerializeField] PlayerCameraFollow playerCam;

	[Header("Explosion")]
	[SerializeField] float DamageMultiplier = 1;
    [SerializeField] GameObject heavyParticle;
    [SerializeField] Transform particleOrigin;
    [SerializeField] AbilityIcon groundPoundIcon;
    [SerializeField] SheepAttack groundPoundAttack;
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] float attackRadius;
    bool canAttack = true;

    PlayerMovement playerMovement;
    Animator animator;
    Rigidbody rb;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        animator = GetComponent<PlayerAnimationController>().playerAnimator;
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (playerMovement.isFalling)
        {
            FMODUnity.RuntimeManager.PlayOneShotAttached(explodeSound, gameObject);
            playerMovement.isFalling = false;
            animator.SetBool("isFalling", false);
            Instantiate(heavyParticle, transform.position, transform.rotation);

            //camera shake!
            playerCam.ShakeCamera(true);

            //for now, do this
            Collider[] enemies = Physics.OverlapSphere(transform.position, attackRadius, enemyLayer);
            foreach (Collider hit in enemies)
            {
				Damageable enemy = hit.GetComponent<Damageable>();

				if (enemy != null)
                {
					float damage = Mathf.Max(0, startFallPos.y - transform.position.y) * DamageMultiplier;
					var dir = -(transform.position - hit.transform.position).normalized;

                    if (GetComponent<PlayerGothMode>().gothMode == PlayerGothMode.GothState.Goth)
                    {
                        groundPoundAttack.BSAttack.damage = baseGroundDamageBlack * damage;
						enemy.TakeDamage(groundPoundAttack.BSAttack, dir);
                    }
                    else
                    {
                        groundPoundAttack.damage = baseGroundDamage * damage;
						enemy.TakeDamage(groundPoundAttack, dir);
                    }
                }
            }
        }
    }
    public void SpawnHeavyParticle()
    {
        GameObject explode = Instantiate(heavyParticle, particleOrigin.position, particleOrigin.rotation);       
    }
    public void HeavySlamDown()
    {
		startFallPos = transform.position;

		animator.SetBool("isFalling", playerMovement.isFalling);

        //called from animator, slam down!!
        rb.AddForce(-rb.velocity, ForceMode.VelocityChange);
        rb.AddForce(Vector3.down * airDownForce);
    }
    public void OnHeavyAttack(InputAction.CallbackContext context)
    {
        //if airborne, do a little lift then slam down into the ground!0
        if (context.started && canAttack && !playerMovement.isLifting)
        {
            RaycastHit info;
            bool didHit = Physics.Raycast(transform.position, Vector3.down, out info, maxHeight, groundMask);
            Debug.Log("GroundPount dist: " + info.distance);
            if (!didHit || info.distance < minHeight)
                return;

			playerMovement.isFalling = true;

            canAttack = false;
            StartCoroutine(GroundPoundCooldown());

            //groundPoundIcon.CooldownUIEffect(coolDown);

            animator.Play(heavyAirAnimation);

            StartCoroutine(SlamDown());


            rb.AddForce(-rb.velocity * 0.5f, ForceMode.VelocityChange);
            rb.AddForce(Vector3.up * airUpForce);
        }
    }

    IEnumerator SlamDown()
    {
        yield return new WaitForSeconds(timeTillSlamDown);
        HeavySlamDown();
    }

    IEnumerator GroundPoundCooldown()
    {
        yield return new WaitForSeconds(coolDown);
        canAttack = true;
    }
}
