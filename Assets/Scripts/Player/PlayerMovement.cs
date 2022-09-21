using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Variables")]
    [SerializeField] float maxMoveSpeed;
    [SerializeField] float acceleration;
    [SerializeField] float haltRate;
    bool isMoving;
    Vector3 moveDirection;
    Vector2 moveValue; //input value
    Rigidbody rb;

    //slope crap I hate slopes
    RaycastHit slopeHit;
    Vector3 slopeMoveDirection;


    [Header("Jump Variables")]
    [SerializeField] float jumpForce;
    [SerializeField] float fallRate;

    [Header("Dash Variables")]
    [SerializeField] float dashForce;
    [SerializeField] float dashAirborneLiftForce;
    [SerializeField] float dashCooldown = 1f;
    bool canDash = true;

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

    #region Debug Stuff
    public void OnDebugRestart(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
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
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        GroundCheck();
        RotatePlayer();
        UpdateAnimation();
    }
    private void FixedUpdate()
    {
        Move();
        SpeedCheck();
    }

    void GroundCheck()
    {
        bool frontCheck = false;
        bool backCheck = false;
        Vector3 frontNormal;
        Vector3 backNormal;

        //set ground check
        RaycastHit hitFront;
        frontCheck = Physics.Raycast(groundCheckOriginFront.position, Vector3.down, out hitFront, groundCheckDistance, groundLayer);
        //if canJump, groundNormal = hit.normal, else groudnnormal = vector3.up  v ternary operater
        frontNormal = frontCheck ? hitFront.normal : Vector3.up;

        RaycastHit hitBack;
        backCheck = Physics.Raycast(groundCheckOriginBack.position, Vector3.down, out hitBack, groundCheckDistance, groundLayer);
        backNormal = backCheck ? hitBack.normal : Vector3.up;

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
        if (moveValue.magnitude != 0 && moveDirection != Vector3.zero) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection), modelRotateSpeed * Time.deltaTime);
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
        if (!GetComponent<PlayerAttacks>().canAttack) // TODO: make this not shit
            return;

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
        if (velocityCheck.magnitude > maxMoveSpeed && canDash)
        {
            Vector3 velocityLimit = velocityCheck.normalized * maxMoveSpeed;
            rb.velocity = new Vector3(velocityLimit.x, rb.velocity.y, velocityLimit.z);
        }

        //halt player if not moving commands
        if (!isMoving && isGrounded && canDash || !GetComponent<PlayerAttacks>().canAttack && isGrounded && canDash)
        {
            rb.AddForce(-rb.velocity * haltRate);
        }

        //apply gravity if falling
        if (!isGrounded) rb.AddForce(Vector3.down * fallRate);
    }


    #region Inputs
    public void OnMove(InputAction.CallbackContext context)
    {
        moveValue = context.ReadValue<Vector2>();
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if(isGrounded)
        {
            animator.Play(jumpAnimation);
            rb.AddForce(Vector3.up * jumpForce);
        }
    }
    public void OnDash(InputAction.CallbackContext context)
    {
        if(canDash)
        {
            canDash = false;

            animator.Play(dashAnimation);

            //halt player
            rb.AddForce(-rb.velocity, ForceMode.VelocityChange);

            //apply lift if in air
            if (!isGrounded) rb.AddForce(transform.up * dashAirborneLiftForce);

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

    IEnumerator DashCooldown()
    {
        canDash = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}
