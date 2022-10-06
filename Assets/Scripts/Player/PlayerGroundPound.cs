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
    [SerializeField] float airUpForce;
    [SerializeField] float airDownForce;
    [SerializeField] PlayerCameraFollow playerCam;

    [Header("Explosion")]
    [SerializeField] GameObject heavyParticle;
    [SerializeField] Transform particleOrigin;
    [SerializeField] AbilityIcon groundPoundIcon;
    [SerializeField] Attack groundPoundAttack;
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
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isFalling)
        {
            isFalling = false;
            animator.SetBool("isFalling", isFalling);
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
                    hit.GetComponent<EnemyAI>().TakeDamage(groundPoundAttack, transform.forward);
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


            rb.AddForce(-rb.velocity * 0.5f, ForceMode.VelocityChange);
            rb.AddForce(Vector3.up * airUpForce);
        }
    }

    IEnumerator GroundPoundCooldown()
    {
        yield return new WaitForSeconds(coolDown);
        canAttack = true;
    }
}
