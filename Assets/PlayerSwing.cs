using UnityEngine;

public class PlayerSwing : MonoBehaviour
{
    [Header("Swing Settings")]
    public float maxSwingDistance = 20f;
    public float spring = 4.5f;
    public float damper = 7f;
    public float massScale = 4.5f;
    public LayerMask swingableLayer;
    public KeyCode swingKey = KeyCode.Mouse0;
    public LineRenderer lineRenderer;

    private SpringJoint swingJoint;
    private Vector3 swingPoint;
    private Camera cam;
    private Rigidbody rb;
    private Vector3 lastSwingVelocity;
    


    void Start()
    {
       
        cam = Camera.main;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Input.GetKeyDown(swingKey))
        {
            TryStartSwing();
        }
        if (Input.GetKeyUp(swingKey))
        {
            StopSwing();
        }
        DrawRope();
    }

    void TryStartSwing()
    {
        
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, maxSwingDistance, swingableLayer))
        {
                swingPoint = hit.point;
                swingJoint = gameObject.AddComponent<SpringJoint>();
                swingJoint.autoConfigureConnectedAnchor = false;
                swingJoint.connectedAnchor = swingPoint;

                float distanceFromPoint = Vector3.Distance(transform.position, swingPoint);

                // The distance the grapple will try to keep from grapple point. 
                swingJoint.maxDistance = distanceFromPoint * 0.8f;
                swingJoint.minDistance = distanceFromPoint * 0.25f;

                // Adjust these values to fit your game.
                swingJoint.spring = spring;
                swingJoint.damper = damper;
                swingJoint.massScale = massScale;

                lineRenderer.enabled = true;
                lineRenderer.positionCount = 2;
        }
    }
    void DrawRope()
    {
        if(!swingJoint) return;

        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, swingPoint);
    }

    void StopSwing()
    {
        if (swingJoint != null)
        {
            // Store the current velocity
            lastSwingVelocity = rb.linearVelocity;

            lineRenderer.positionCount = 0;
            Destroy(swingJoint);

            // Optionally, re-apply the velocity to ensure momentum is kept
            rb.linearVelocity = lastSwingVelocity;

            // Optional: Add a small boost if you want
            // rb.AddForce(lastSwingVelocity.normalized * 2f, ForceMode.VelocityChange);
        }
    }

    public bool IsSwinging => swingJoint != null;
} 