using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestJump : MonoBehaviour
{
    public float jumpForce = 5f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (Input.GetKeyDown("space"))
        {
            StartJump(); 
        }
    }

    void StartJump()
    {
        // Calculate jump direction (up and forward)
        Vector3 jumpDirection = (transform.up + transform.forward).normalized;
        rb.AddForce(jumpDirection * jumpForce, ForceMode.Impulse);

        // Invoke("EndJump", 1f); // Adjust time as needed
    }
}
