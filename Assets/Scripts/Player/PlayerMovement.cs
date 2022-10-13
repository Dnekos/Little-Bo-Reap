using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [Header("Temp sounds")]
    [SerializeField] FMODUnity.EventReference jumpSound;
    [SerializeField] FMODUnity.EventReference dashSound;
	
    [Header("Movement Variables")]
    [SerializeField] float maxMoveSpeed;
    [SerializeField] float acceleration;
    [SerializeField] float haltRate;
    [SerializeField] ParticleSystem runParticles;
    [SerializeField] float timeBetweenRunParticles = 0.1f;
    [SerializeField] float currentRunTime = 0f;
    bool isMoving;
    Vector3 moveDirection;
    Vector2 moveValue; // input value
    Rigidbody rb;

    // slope crap I hate slopes
    RaycastHit slopeHit;
    Vector3 slopeMoveDirection;


    [Header("Jump Variables")]
    [SerializeField] float jumpForce;
    [SerializeField] float fallRate;
    [SerializeField] ParticleSystem jumpParticles;

    [Header("Dash Variables")]
    [SerializeField] float dashForce;
    [SerializeField] float dashAirborneLiftForce;
    [SerializeField] float dashCooldown = 1f;
    [SerializeField] ParticleSystem dashTrail;
    bool canDash = true;

    [Header("Dash Slow Time Variables")]
    [SerializeField] float dashSlowTimescale = 0.5f;
    [SerializeField] float dashSlowLength = 0.25f;
    [SerializeField] GameObject slowTimeVolume;
    [SerializeField] GameObject slowTimeUI;
    [SerializeField] float dashTriggerRadius = 5f;
    [SerializeField] LayerMask enemyAttackLayer;
    float defaultTimescale = 1.0f;

    [Header("Ground Check")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Transform groundCheckOriginFront;
    [SerializeField] Transform groundCheckOriginBack;
    [SerializeField] float groundCheckDistance;
    public bool isGrounded = false;

    [Header("Model Orientation")]
    [SerializeField] float modelRotateSpeed = 10f;
    [SerializeField] float orientCheckDistance = 5f;
    [SerializeField] Transform playerOrientation;
    Vector3 modelRotateNormal;

    [Header("Animations")]
    [SerializeField] string jumpAnimation;
    [SerializeField] string dashAnimation;
    Animator animator;

	PlayerHealth health;
	
    #region Debug Stuff
    public void OnDebugRestart(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    public void OnDebugQuit(InputAction.CallbackContext context)
    {
        Application.Quit();
    }
    #endregion



    private void Awake()
    {
        //set this as player in game manager
        GameManager.Instance.SetPlayer(this.transform);

    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();
		health = GetComponent<PlayerHealth>();
        animator = GetComponent<PlayerAnimationController>().playerAnimator;
    }

    private void Update()
    {
        GroundCheck();
        UpdateAnimation();
    }
    private void FixedUpdate()
    {
		RotatePlayer();

        Move();
        SpeedCheck();
    }

    void GroundCheck()
    {
        bool frontCheck = false;
        bool backCheck = false;
       //Vector3 frontNormal;
       //Vector3 backNormal;
       //
       ////set ground check
       //RaycastHit hitFront;
       //frontCheck = Physics.Raycast(groundCheckOriginFront.position, Vector3.down, out hitFront, groundCheckDistance, groundLayer);
       ////if canJump, groundNormal = hit.normal, else groudnnormal = vector3.up  v ternary operater
       //frontNormal = frontCheck ? hitFront.normal : Vector3.up;
       //
       //RaycastHit hitBack;
       //backCheck = Physics.Raycast(groundCheckOriginBack.position, Vector3.down, out hitBack, groundCheckDistance, groundLayer);
       //backNormal = backCheck ? hitBack.normal : Vector3.up;

        frontCheck = Physics.CheckSphere(groundCheckOriginFront.position, groundCheckDistance, groundLayer);
        backCheck = Physics.CheckSphere(groundCheckOriginBack.position, groundCheckDistance, groundLayer);

        isGrounded = frontCheck || backCheck;
    }
    void RotatePlayer()
    {
        //am i on a slope?
        slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);


        //bool frontCheck = false;
        //bool backCheck = false;
        //Vector3 frontNormal;
        //Vector3 backNormal;
        //
        ////set rotate check
        //RaycastHit hitFront;
        //frontCheck = Physics.Raycast(groundCheckOriginFront.position, Vector3.down, out hitFront, orientCheckDistance, groundLayer);
        ////if canJump, groundNormal = hit.normal, else groudnnormal = vector3.up  v ternary operater
        //frontNormal = frontCheck ? hitFront.normal : Vector3.up;
        //
        //RaycastHit hitBack;
        //backCheck = Physics.Raycast(groundCheckOriginBack.position, Vector3.down, out hitBack, orientCheckDistance, groundLayer);
        //backNormal = backCheck ? hitBack.normal : Vector3.up;
        //
        //modelRotateNormal = (frontNormal + backNormal) * 0.5f;
        //
        ////Up is just the rotate normal
        //Vector3 up = modelRotateNormal;
        ////Make sure the velocity is normalized
        //Vector3 vel = transform.forward.normalized;
        ////Project the two vectors using the dot product
        //Vector3 forward = vel - up * Vector3.Dot(vel, up);
        //
        ////Set the rotation with relative forward and up axes
        //rb.MoveRotation(Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(forward.normalized, up), Time.deltaTime * modelRotateSpeed));
        //if (moveValue.magnitude != 0 && moveDirection != Vector3.zero) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.Cross(modelRotateNormal, moveDirection)), modelRotateSpeed * Time.deltaTime);

        //rotate the player body
        if (moveValue.magnitude != 0 && moveDirection != Vector3.zero)
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection), modelRotateSpeed * Time.deltaTime);
    }
    void UpdateAnimation()
    {
        animator.SetBool("isMoving", isMoving);
    }

    bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, orientCheckDistance, groundLayer))
        {
            if (slopeHit.normal != Vector3.up)
            {
                return true;
            }
            else return false;
        }
        return false;
    }
    void Move()
    {
		// cant move when stunned
		if (health.HitStunned)
			return;

        currentRunTime += Time.deltaTime;

        //do clouds if running and grounded
        if (isGrounded && isMoving && currentRunTime > timeBetweenRunParticles)
        {
            currentRunTime = 0f;
            runParticles.Play();
        }


        //moveDirection = playerOrientation.forward * moveValue.x + playerOrientation.right * -moveValue.y;
        //rb.AddForce(moveDirection * acceleration);

        moveDirection = playerOrientation.forward * moveValue.y + playerOrientation.right * moveValue.x;

        if (OnSlope())
        {
            rb.AddForce(slopeMoveDirection * acceleration);
        }
        else
        {
            rb.AddForce(moveDirection * acceleration);
        }

        //rb.AddForce(Vector3.Cross(modelRotateNormal, (moveDirection * acceleration)));
        // Vector3.Cross(modelRotateNormal, -(transform.right * moveValue * moveSpeed))
    }
    void SpeedCheck()
    {
        //check if moving
        if (moveValue.magnitude != 0) isMoving = true;
        else isMoving = false;

        //check current velocity
        Vector3 velocityCheck = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        //limit velocity if over speed
        if (velocityCheck.magnitude > maxMoveSpeed && canDash && !health.HitStunned)
        {
            Vector3 velocityLimit = velocityCheck.normalized * maxMoveSpeed;
            rb.velocity = new Vector3(velocityLimit.x, rb.velocity.y, velocityLimit.z);
        }

        //halt player if not moving commands
        if (!isMoving && isGrounded && canDash && !health.HitStunned)
        {
            rb.AddForce(-rb.velocity * haltRate);
        }

        //apply gravity if falling
        if (!isGrounded)
			rb.AddForce(Vector3.down * fallRate);
    }


    #region Inputs
    public void OnMove(InputAction.CallbackContext context)
    {
        moveValue = context.ReadValue<Vector2>();
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if(isGrounded && context.started)
        {
			//TEMP SOUND
			FMODUnity.RuntimeManager.PlayOneShotAttached(jumpSound, gameObject);

            //play particles
            jumpParticles.Play();

            animator.Play(jumpAnimation);
            rb.AddForce(Vector3.up * jumpForce);
        }
    }
    public void OnDash(InputAction.CallbackContext context)
    {
        if(canDash && context.started && !health.HitStunned)
        {
            canDash = false;

            animator.Play(dashAnimation);

			//TEMP SOUND
			FMODUnity.RuntimeManager.PlayOneShotAttached(dashSound,gameObject);

            //halt player
            rb.AddForce(-rb.velocity, ForceMode.VelocityChange);

            //apply lift if in air
            if (!isGrounded) rb.AddForce(transform.up * dashAirborneLiftForce);

            //dash particles
            //dashParticles.Play();

            //apply invuln if avoiding attack
            DidDashAvoidAttack();

            //apply the dash
            if (moveDirection.magnitude == 0)
            {
                rb.AddForce(playerOrientation.forward * dashForce);
            }
            else
            {
                rb.AddForce(moveDirection * dashForce);
            }


            StartCoroutine(DashCooldown());
        }
    }
    #endregion

    #region Dash

    IEnumerator DashCooldown()
    {
        dashTrail.Play();
        canDash = false;
        yield return new WaitForSeconds(dashCooldown);
        dashTrail.Stop();
        canDash = true;
    }
    public void DidDashAvoidAttack()
    {
        if (Physics.CheckSphere(transform.position, dashTriggerRadius, enemyAttackLayer))
        {
            StartCoroutine(DashSlowTime());
        }
    }
    IEnumerator DashSlowTime()
    {
        gameObject.layer = LayerMask.NameToLayer("PlayerInvulnerable");
        Time.timeScale = dashSlowTimescale;
        Time.fixedDeltaTime = 0.02F * Time.timeScale;
        slowTimeVolume.SetActive(true);
        slowTimeUI.SetActive(true); 
        health.isInvulnerable = true;

        yield return new WaitForSeconds(dashSlowLength);

        gameObject.layer = LayerMask.NameToLayer("Player");
        Time.timeScale = defaultTimescale;
        Time.fixedDeltaTime = 0.02F;
        slowTimeVolume.SetActive(false);
        slowTimeUI.SetActive(false);
        health.isInvulnerable = false;
        
    }

    #endregion
}
