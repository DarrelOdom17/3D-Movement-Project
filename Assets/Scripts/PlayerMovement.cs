using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("KeyBinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.C;


    [Header("Speed Controls")]
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;
    public float groundDrag;
    private float moveSpeed;


    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMulitplier;


    [Header("Crouching")]
    public float crouchSpeed;
    private float normalPlayerScale;
    public float crouchHeight;
    
    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask thisisGround;


    [Header("Slope Movement")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    

    [Header("Bool Checks")]
    public bool isGrounded;
    public bool readyToJump;
    public bool isCrouching;
    public bool TouchingCeiling = false;
    public bool previousCrouchCheck;
    public bool exitSlope;
    public bool isSliding;


    // *** VARIOUS OTHER VARIABLES ***/
    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDireciton;

    Rigidbody rb;

    public MovementState state;

    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        sliding,
        slopeSlide
    }

    private void Start()
    {
        // Finds referecne to the players rigidbody component and then freezes it so that the player stays upright
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
        // Records the players default "Y" scale for use later
        normalPlayerScale = transform.localScale.y;
    }

    private void Update()
    {
        // Checks if the player is touching a ceiling at the beginning of every frame
        TouchingCeiling = Physics.Raycast(transform.position, Vector3.up, playerHeight * 0.5f + 0.2f, thisisGround);

        MovementInput();
        MovementStates();
        SpeedControl();

        // Uses a raycast to check if the player is in contact with the ground layermask
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, thisisGround);
        // TouchingCeiling = Physics.Raycast(transform.position, Vector3.up, playerHeight * 0.5f + 0.2f, thisisGround);

        // Applies drag to the player if the "isGrounded" bool is true otherwise the player feels like they are on ice
        if (isGrounded)
            rb.drag = groundDrag;

        else
            rb.drag = 0;

        // Changes the boolean for previousCrouchCheck to the same as touchingceiling
        // Couldn't think of a simple way for the player to automatically un-crouch if the
        // crouch key was released while under something. Ideas???
        previousCrouchCheck = TouchingCeiling;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    // Uses the horizontal and vertical inputs for player movement
    private void MovementInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && readyToJump && isGrounded)
        {   
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetKey(crouchKey) && Input.GetKey(sprintKey) == false && isGrounded)
        {
            Crouch();
        }

        if (Input.GetKeyUp(crouchKey))
        {
            ResetCrouch();
        }

        // Checks if the previousCrouchCheck bool is true and the current raycast for touchingceiling is false
        // then resets the crouch function
        if (isCrouching && previousCrouchCheck && !TouchingCeiling)
        {
            ResetCrouch();
        }
    }

    // Controls the player movement state 
    private void MovementStates()
    {
        // State 1 - Sprinting
        if (Input.GetKey(sprintKey) && isGrounded)
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }

        // State 2 - Crouching
        else if (isCrouching)
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }

        // State 3 - Sliding
        else if (isSliding && !OnSlope())
        {
            state = MovementState.sliding;
            moveSpeed = sprintSpeed;
        }

        else if (isSliding && OnSlope())
        {
            state = MovementState.slopeSlide;
            moveSpeed = slideSpeed;
        }

        // State 4 - Walking
        else if (isGrounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }

    }
    private void MovePlayer()
    {
        moveDireciton = orientation.forward * verticalInput + orientation.right * horizontalInput;
        //rb.AddForce(moveDireciton.normalized * moveSpeed * 10f, ForceMode.Force);

        // Movement on slopes
        if (OnSlope() && !exitSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDireciton) * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        // Movement on Ground
        else if (isGrounded)
            rb.AddForce(moveDireciton.normalized * moveSpeed * 10f, ForceMode.Force);

        // Movemetn in Air
        else if (!isGrounded)
            rb.AddForce(moveDireciton.normalized * moveSpeed * 10f * airMulitplier, ForceMode.Force);

        // Turns off gravity while on a slope (keeps the player from sliding while on a slope)
        rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        // Controls the players movement speed on slopes. Otherwise the player would move faster than when on the ground
        if (OnSlope() && !exitSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }
        
        // Limits the players velocity (helps will player momentum when in the air)
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }
    
    private void Jump()
    {
        // Allows the player to jump when on a slope
        exitSlope = true;

        // Makes sure to reset the player's vertical velocity with every jump
        rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
        //rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
        exitSlope = false;
    }

    // Changes the player y.transform to simulate crouching
    private void Crouch()
    {
        transform.localScale = new Vector3(transform.localScale.x, crouchHeight, transform.localScale.z);

        // Adds a downward force to make sure the player is grounded after altering the y.transform
        rb.AddForce(Vector3.down * 5f, ForceMode.Force);
        isCrouching = true;
    }

    // Resets the player y.transform back to it's default size
    private void ResetCrouch()
    {
        if (TouchingCeiling)
            isCrouching = true;
        else
        {
            transform.localScale = new Vector3(transform.localScale.x, normalPlayerScale, transform.localScale.z);
            isCrouching = false;
        }
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight / 2 + 0.5f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
}
