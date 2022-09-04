using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttacks : MonoBehaviour
{
    //light attack class
    [System.Serializable]
    public class LightAttack
    {
        [Header("Light Attack Variables")]
        public float damage;
        public float knockback;
        public float airborneLift;
        public string animation;
    }
 
    [Header("Light Attack Variables")]
    [SerializeField] List<LightAttack> lightAttacks;
    [SerializeField] float timeframeToChainAttacks = 0.75f;
    bool canAttack = true;
    int currentAttackChain = 0;

    [Header("Grounded Heavy Attack Variables")]
    [SerializeField] float heavyDamage;
    [SerializeField] float heavyKnockback;
    [SerializeField] float heavyChargeTime = 0.5f;
    [SerializeField] string heavyChargeAnimation;
    [SerializeField] string heavyAttackAnimation;
    [SerializeField] GameObject heavyParticle;
    [SerializeField] Transform particleOrigin;
    float heavyChargeTimeCurrent = 0f;
    bool isChargingHeavyAttack = false;

    [Header("Airborne Heavy Attack Variables")]
    [SerializeField] float heavyAirDamage;
    [SerializeField] float heavyAirKnockback;
    [SerializeField] string heavyAirAnimation;
    [SerializeField] float airUpForce;
    [SerializeField] float airDownForce;
    bool isFalling = false;

    PlayerMovement playerMovement;
    Animator animator;
    Rigidbody rb;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }


    #region Light Attack
    public void EnableLightAttack()
    {
        canAttack = true;
    }
    public void DisableLightAttack()
    {
        canAttack = false;
    }
    IEnumerator LightAttackChain()
    {
        yield return new WaitForSeconds(timeframeToChainAttacks);
        currentAttackChain = 0;
    }

    public void OnLightAttack(InputAction.CallbackContext context)
    {
        //if we can light attack, attack!
        if(context.started && canAttack)
        {
            //stop attack chain reset coroutine
            StopCoroutine("LightAttackChain");

            //do current light attack
            animator.Play(lightAttacks[currentAttackChain].animation);
            //TODO set knockback and damage of crook depending on what attack it is

            //apply lift if in air
            if (!playerMovement.isGrounded) rb.AddForce(Vector3.up * lightAttacks[currentAttackChain].airborneLift);

            //increase attack chain
            currentAttackChain++;
            if (currentAttackChain >= lightAttacks.Count) currentAttackChain = 0;

            //start reset coroutine
            StartCoroutine("LightAttackChain");
        }
    }
    #endregion

    #region Heavy Attack

    private void Update()
    {
        if (isChargingHeavyAttack)
        {
            heavyChargeTimeCurrent += Time.deltaTime;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(isFalling)
        {
            isFalling = false;
            animator.SetBool("isFalling", isFalling);
            Instantiate(heavyParticle, transform.position, transform.rotation);
            EnableLightAttack();
        }
    }
    public void SpawnHeavyParticle()
    {
        Instantiate(heavyParticle, particleOrigin.position, particleOrigin.rotation);
    }
    public void HeavySlamDown()
    {
        //called from animator, slam down!!
        rb.AddForce(-rb.velocity, ForceMode.VelocityChange);
        rb.AddForce(Vector3.down * airDownForce);
    }
    public void OnHeavyAttack(InputAction.CallbackContext context)
    {
        //if airborne, do a little lift then slam down into the ground!
        if(context.started && canAttack && !playerMovement.isGrounded)
        {
            DisableLightAttack();
            isFalling = true;

            animator.Play(heavyAirAnimation);
            animator.SetBool("isFalling", isFalling);

            rb.AddForce(Vector3.up * airUpForce);
        }

        //charge heavy attack if grounded
        if(context.started && canAttack && playerMovement.isGrounded)
        {
            isChargingHeavyAttack = true;
            DisableLightAttack();
            animator.Play(heavyChargeAnimation);
            animator.SetBool("isChargingHeavyAttack", isChargingHeavyAttack);
        }

        if(context.canceled && heavyChargeTimeCurrent >= heavyChargeTime)
        {
            heavyChargeTimeCurrent = 0f;
            isChargingHeavyAttack = false;

            //do heavy attack
            animator.Play(heavyAttackAnimation);
            //TODO set knockback and damage of crook depending on what attack it is

            animator.SetBool("isChargingHeavyAttack", isChargingHeavyAttack);
        }
        else if(context.canceled)
        {
            heavyChargeTimeCurrent = 0f;
            isChargingHeavyAttack = false;
            EnableLightAttack();
            animator.SetBool("isChargingHeavyAttack", isChargingHeavyAttack);
        }
    }


    #endregion
}