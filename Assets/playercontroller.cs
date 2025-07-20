using UnityEngine;

public class playercontroller : MonoBehaviour
{
    public Rigidbody rb;
    public float speed = 12f;
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z) * speed * Time.deltaTime;

    }
}
