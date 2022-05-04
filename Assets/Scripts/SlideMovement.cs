using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SlideMovement : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    private PlayerMovement pMovement;

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    private float slideTimer;

    public float playerSlideScale;
    private float normalPlayerScale;

    [Header("KeyCodes")]
    public KeyCode slideKey = KeyCode.LeftControl;

    private float horizontalInput;
    private float verticalInput;

    //[Header("Bool Checks")]
    //public bool isSliding;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pMovement = GetComponent<PlayerMovement>();

        normalPlayerScale = playerObj.localScale.y;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Checks for player input in movement and if found starts sliding
        if (Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0))
            StartSlide();

        // Resets the sliding state when the keystroke is released
        if (Input.GetKeyUp(slideKey) && pMovement.isSliding)
            ExitSlide();
    }

    private void FixedUpdate()
    {
        // Because the sliding based movement is physics based it should be called in a fixed update
        if (pMovement.isSliding)
            SlideMovementControl();
    }

    // Checks for a bool for sliding and if true changes the players y scale and starts the slide timer
    private void StartSlide()
    {
        pMovement.isSliding = true;
        playerObj.localScale = new Vector3(playerObj.localScale.x, playerSlideScale, playerObj.localScale.z);

        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        slideTimer = maxSlideTime;
    }

    // Resets the slide bool to false and reverts the player y scale to its normal size on exit of the state
    private void ExitSlide()
    {
        pMovement.isSliding = false;
        playerObj.localScale = new Vector3(playerObj.localScale.x, normalPlayerScale, playerObj.localScale.z);
    }

    // Using an input direction this allows the player to slide in any direction that the wish
    private void SlideMovementControl()
    {
        Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // If sliding on a normal surface
        if (!pMovement.OnSlope() || rb.velocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);
            slideTimer -= Time.deltaTime;
        }

        // If sliding down a slope
        // Removing the slide timer allows the player to continuously slide down slopes
        else
        {
            rb.AddForce(pMovement.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }

        if (slideTimer <= 0)
            ExitSlide();
    }
}
