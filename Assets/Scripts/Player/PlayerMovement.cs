using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [Header("Sounds")]
    [SerializeField] FMODUnity.EventReference jumpSound;
    [SerializeField] FMODUnity.EventReference dashSound;
	[SerializeField] FMODUnity.StudioEventEmitter walker;

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
    [SerializeField] float superJumpPreventionTimer = 0.1f;
    bool canJump = true;
	[SerializeField] float CoyoteTime = 0.2f;
	float CoyoteTimer;

	// this will need some retooling
	//[SerializeField] float JumpBufferTime = 0.2f;
	//float BufferTimer;



	[Header("Sheep Lift")]
	[SerializeField] float LiftMaxSpeedModifier = 0.5f;
	public float LiftSpeed = 5;
	public bool isLifting = false;
	[HideInInspector] public bool CanLift = true;

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
    bool wasGroundedLastFrame = true;
    public bool isGrounded = false;

    [Header("Model Orientation")]
    [SerializeField] float modelRotateSpeed = 10f;
    [SerializeField] float orientCheckDistance = 5f;
    [SerializeField] Transform playerOrientation;
    Vector3 modelRotateNormal;

    [Header("Animations")]
    [SerializeField] string jumpAnimation;
    [SerializeField] string dashAnimation;
    [SerializeField] string landingAnimation;
    Animator animator;

	PlayerHealth health;
	PlayerSheepLift liftcontroller;
	PlayerGroundPound groundPound;
	
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
		liftcontroller = GetComponent<PlayerSheepLift>();
		groundPound = GetComponent<PlayerGroundPound>();

	}

    private void Update()
    {
		Debug.DrawLine(transform.position, transform.position + slopeMoveDirection);

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

        frontCheck = Physics.CheckSphere(groundCheckOriginFront.position, groundCheckDistance, groundLayer);
        backCheck = Physics.CheckSphere(groundCheckOriginBack.position, groundCheckDistance, groundLayer);

        isGrounded = frontCheck || backCheck;

		// coyote counts down when in air, allows for late inputs
		if (isGrounded)
		{
			CoyoteTimer = CoyoteTime;
		}
		else
			CoyoteTimer -= Time.deltaTime;

		// player should be able to activate lift again once they are back on the ground
		if (!CanLift && isGrounded)
			CanLift = true;
	}
    void RotatePlayer()
    {
        //am i on a slope?
        slopeMoveDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;

        //rotate the player body
        if (moveValue.magnitude != 0 && moveDirection != Vector3.zero)
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection), modelRotateSpeed * Time.deltaTime);
    }
    void UpdateAnimation()
    {
        animator.SetBool("isMoving", isMoving && !isLifting);
        animator.SetBool("isGrounded", isGrounded);

        //check if was not grounded last frame and is grounded this frame

        if(isGrounded && !wasGroundedLastFrame)
        {
            animator.Play(landingAnimation);
        }
        wasGroundedLastFrame = isGrounded;
    }

    bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, orientCheckDistance, groundLayer))
        {
			return slopeHit.normal != Vector3.up;
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

			if (!walker.IsPlaying())
				walker.Play();
		}

		// stop playing footsteps when not walking
		if (walker.IsPlaying() && (!isGrounded || !isMoving || !canDash))
		{
			walker.Stop();
		}


		moveDirection = playerOrientation.forward * moveValue.y + playerOrientation.right * moveValue.x;

		if (OnSlope())
        {
			var slopeRotation = Quaternion.FromToRotation(Vector3.up, slopeHit.normal);

            rb.AddForce(slopeRotation * moveDirection * acceleration);
        }
        else
        {
			rb.AddForce(moveDirection * acceleration);
        }

    }
    void SpeedCheck()
    {
        //check if moving
		isMoving = moveValue.magnitude != 0;

        //check current velocity
        Vector3 velocityCheck = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        //limit velocity if over speed
        if (velocityCheck.magnitude > maxMoveSpeed * (isLifting ? LiftMaxSpeedModifier : 1)
			&& canDash && !health.HitStunned)
        {
            Vector3 velocityLimit = velocityCheck.normalized * maxMoveSpeed * (isLifting ? LiftMaxSpeedModifier : 1);
            rb.velocity = new Vector3(velocityLimit.x, rb.velocity.y, velocityLimit.z);
        }

        //halt player if not moving commands
        if (!isMoving && isGrounded && canDash && !health.HitStunned)
        {
            rb.AddForce(-rb.velocity * haltRate);
        }

		//apply gravity if falling
		if (isLifting)
		{
			rb.velocity = new Vector3(rb.velocity.x, LiftSpeed, rb.velocity.z).normalized * LiftSpeed;
		}
		else
			rb.AddForce(Vector3.down * fallRate);
    }


    #region Inputs
    public void OnMove(InputAction.CallbackContext context)
    {
        moveValue = context.ReadValue<Vector2>();
    }
    public void OnJump(InputAction.CallbackContext context)
    {
		if (CoyoteTimer > 0f && context.started && canJump)
		{
            //prevent super jumps
            StartCoroutine(SuperJumpPrevention());

			// SOUND
			FMODUnity.RuntimeManager.PlayOneShotAttached(jumpSound, gameObject);

			// play particles
			jumpParticles.Play();

			animator.Play(jumpAnimation);
			rb.AddForce(Vector3.up * jumpForce);
		}
		else if (context.started && CanLift && !groundPound.isFalling)
		{
			if (liftcontroller.StartLifting())
			{

				rb.AddForce(Vector3.down * rb.velocity.y, ForceMode.VelocityChange);

				CanLift = false;
				// lifting = true is now in PlayerSheepLift

				StartCoroutine(GetComponent<PlayerSheepLift>().PlayerPath());
			}
		}
		else if (context.canceled && isLifting)
		{
			isLifting = false;
		}
    }

    IEnumerator SuperJumpPrevention()
    {
        canJump = false;
        yield return new WaitForSeconds(superJumpPreventionTimer);
        canJump = true;
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if(canDash && !isLifting && context.started && !health.HitStunned)
        {
            canDash = false;

            animator.Play(dashAnimation);

			// SOUND
			FMODUnity.RuntimeManager.PlayOneShotAttached(dashSound,gameObject);

            //halt player
            rb.AddForce(-rb.velocity, ForceMode.VelocityChange);

            //apply lift if in air
            if (!isGrounded) 
				rb.AddForce(transform.up * dashAirborneLiftForce);

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
