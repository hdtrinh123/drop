using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerMovementTutorial : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;

    public float groundDrag = 0f; // Set to 0 for momentum-based movement, increase for more friction

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;
    

    [HideInInspector] public float walkSpeed;
    [HideInInspector] public float sprintSpeed;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    [HideInInspector] public bool grounded;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;
    private PlayerSwing swingScript;
    public float swingMaxSpeed = 1000f; // Very high max speed when swinging
    private WallRunning wallrunScript;
    public float wallrunMaxSpeed = 15f;
    
    // Player states
    [HideInInspector] public bool wallrunning;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
        swingScript = GetComponent<PlayerSwing>();
        wallrunScript = GetComponent<WallRunning>();
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

        MyInput();
        SpeedControl();

        // handle drag
        if (grounded)
            rb.linearDamping = groundDrag; // Use drag for ground friction
        else
            rb.linearDamping = 0; // No drag in air for fluid movement
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if(Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        bool isSwinging = swingScript != null && swingScript.IsSwinging;

        if (grounded || isSwinging)
        {
            float forceMultiplier = grounded ? 1f : airMultiplier;
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * forceMultiplier, ForceMode.Force);
        }
        else
        {
            // Small air control when not swinging
            float airControlMultiplier = 0.2f; // Tweak this value (0.1-0.3) for desired air control
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airControlMultiplier, ForceMode.Force);
        }
    }

    private void SpeedControl()
    {
        float maxSpeed = moveSpeed;
        bool isSwinging = swingScript != null && swingScript.IsSwinging;
        bool isWallrunning = wallrunScript != null && wallrunning;

        if (isSwinging)
            maxSpeed = swingMaxSpeed;
        else if (isWallrunning)
            maxSpeed = wallrunMaxSpeed;

        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // Only clamp velocity if grounded, swinging, or wallrunning
        if ((grounded || isSwinging || isWallrunning) && flatVel.magnitude > maxSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        // reset y velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;
    }
}