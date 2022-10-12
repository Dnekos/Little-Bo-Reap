using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerGroundPound : MonoBehaviour
{
    [Header("Ground Pound Variables")]
    [SerializeField] FMODUnity.EventReference explodeSound;
    [SerializeField] string heavyAirAnimation;
    [SerializeField] float coolDown = 3f;
    [SerializeField] float timeTillSlamDown = 0.25f;
    [SerializeField] float airUpForce;
    [SerializeField] float airDownForce;
    [SerializeField] PlayerCameraFollow playerCam;

    [Header("Explosion")]
    [SerializeField] GameObject heavyParticle;
    [SerializeField] Transform particleOrigin;
    [SerializeField] AbilityIcon groundPoundIcon;
    [SerializeField] SheepAttack groundPoundAttack;
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] float attackRadius;
    bool isFalling = false;
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
        if (isFalling)
        {
            isFalling = false;
            animator.SetBool("isFalling", false);
            Instantiate(heavyParticle, transform.position, transform.rotation);

            //TEMP SOUND
            FMODUnity.RuntimeManager.PlayOneShotAttached(explodeSound,gameObject);

            //camera shake!
            playerCam.ShakeCamera(true);

            //for now, do this
            Collider[] enemies = Physics.OverlapSphere(transform.position, attackRadius, enemyLayer);
            foreach (Collider hit in enemies)
            {
                if (hit.GetComponent<EnemyAI>() != null)
                {
                    var dir = -(transform.position - hit.transform.position).normalized;
                    if (GetComponent<PlayerGothMode>().isGothMode) hit.GetComponent<EnemyAI>().TakeDamage(groundPoundAttack, dir);
                    else hit.GetComponent<EnemyAI>().TakeDamage((Attack)groundPoundAttack, dir);
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
        isFalling = true;
        animator.SetBool("isFalling", isFalling);

        //called from animator, slam down!!
        rb.AddForce(-rb.velocity, ForceMode.VelocityChange);
        rb.AddForce(Vector3.down * airDownForce);
    }
    public void OnHeavyAttack(InputAction.CallbackContext context)
    {
        //if airborne, do a little lift then slam down into the ground!
        if (context.started && canAttack)
        {
            canAttack = false;
            StartCoroutine(GroundPoundCooldown());

            groundPoundIcon.CooldownUIEffect(coolDown);

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
