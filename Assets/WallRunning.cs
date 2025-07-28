using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [Header("Wallrunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float wallClimbSpeed;
    public float maxWallRunTime;
    private float wallRunTimer;

    [Header("Input")]
    public KeyCode upwardsRunKey = KeyCode.LeftShift;
    public KeyCode downwardsRunKey = KeyCode.LeftControl;
    private bool upwardsRunning;
    private bool downwardsRunning;
    private float horizontalInput;
    private float verticalInput;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    public float minWallAngleDifference = 30f; // Minimum angle difference required to wallrun again
    private RaycastHit leftWallhit;
    private RaycastHit rightWallhit;
    private bool wallLeft;
    private bool wallRight;
    private Vector3 lastWallNormal; // Track the last wall normal
    private bool canWallRun = true; // Whether player can start a new wallrun

    [Header("References")]
    public Transform orientation;
    private PlayerMovementTutorial pm;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovementTutorial>();
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();
    }

    private void FixedUpdate()
    {
        if (pm.wallrunning)
            WallRunningMovement();
    }

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallhit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallhit, wallCheckDistance, whatIsWall);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private bool IsWallAngleDifferent(Vector3 currentWallNormal)
    {
        // If this is the first wallrun, allow it
        if (lastWallNormal == Vector3.zero)
            return true;
        
        // Calculate angle between current wall normal and last wall normal
        float angleDifference = Vector3.Angle(currentWallNormal, lastWallNormal);
        
        // Return true if angle difference is greater than minimum required
        return angleDifference >= minWallAngleDifference;
    }

    private void StateMachine()
    {
        // Getting Inputs
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        upwardsRunning = Input.GetKey(upwardsRunKey);
        downwardsRunning = Input.GetKey(downwardsRunKey);

        // State 1 - Wallrunning
        if((wallLeft || wallRight) && verticalInput > 0 && AboveGround() && !pm.grounded && canWallRun)
        {
            // Check if current wall has different angle than last wall
            Vector3 currentWallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;
            
            if (!pm.wallrunning && IsWallAngleDifferent(currentWallNormal))
            {
                StartWallRun();
            }
        }

        // State 2 - None
        else
        {
            if (pm.wallrunning)
                StopWallRun();
        }
    }

    private void StartWallRun()
    {
        pm.wallrunning = true;
        wallRunTimer = maxWallRunTime;
        
        // Store the current wall normal
        lastWallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;
    }

    private void WallRunningMovement()
    {
        rb.useGravity = false;
        
        // Reset vertical velocity to prevent falling
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y / 1.001f, rb.linearVelocity.z);

        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        // forward force
        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        // upwards/downwards force
        if (upwardsRunning)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, wallClimbSpeed, rb.linearVelocity.z);
        if (downwardsRunning)
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, -wallClimbSpeed, rb.linearVelocity.z);

        // push to wall force - only when not trying to move away from wall
        if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
            rb.AddForce(-wallNormal * 2, ForceMode.Force); // Reduced force to prevent sticking

        // Wall run timer
        wallRunTimer -= Time.deltaTime;
        if (wallRunTimer <= 0)
            StopWallRun();
    }

    private void StopWallRun()
    {
        pm.wallrunning = false;
        rb.useGravity = true; // Restore gravity when stopping wallrun
        
        // Prevent immediate wallrunning on the same wall
        canWallRun = false;
        StartCoroutine(ResetWallRunAbility());
    }

    private IEnumerator ResetWallRunAbility()
    {
        // Wait a short time before allowing wallrunning again
        yield return new WaitForSeconds(0.5f);
        canWallRun = true;
    }
}
