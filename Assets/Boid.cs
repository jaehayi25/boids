using UnityEngine;
using System.Collections.Generic;

public class Boid : MonoBehaviour
{
    public float maxSpeed = 5f;
    public float maxSteerForce = 3f;
    public float neighborhoodRadius = 5f;
    public float separationWeight = 1f;
    public float alignmentWeight = 1f;
    public float cohesionWeight = 1f;
    public float mouseAttractionWeight = 1f; 
    public float fixedHeight = 0f;
    public float rotationSpeed = 5f;
    public float modelRotationOffset = 90f;

    private bool boidOn = true;

    [HideInInspector]
    public List<Boid> neighbors = new List<Boid>();

    private Rigidbody rb;
    private static Vector3 mousePosition;
    
    public float jumpDuration = 1.5f;
    public float jumpHeight = 2f;

    private bool isJumping = false;
    private Vector3 jumpStartPosition;
    private Vector3 jumpEndPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Vector3 position = transform.position;
        position.y = fixedHeight;
        transform.position = position;
    }


    void Update()
    {
        /*
        if (boidOn)
        {
            UpdateMousePosition();
            RotateTowardsMouse();
        }
        */
        if (boidOn) // check if entered jump area
        {
            StartJump();
        }
        if (isJumping)
        {
            UpdateJump(); 
        }
    }

    void StartJump()
    {
        isJumping = true;
        boidOn = false;

        jumpStartPosition = transform.position;
        jumpEndPosition = transform.position + new Vector3(2f, 2f, 2f);

        // Trigger jump animation
        /*
        if (animator)
        {
            animator.SetTrigger("Jump");
        }
        */
    }

    void UpdateJump()
    {
        Vector3 num = transform.position - jumpStartPosition; num.y = 0f;
        Vector3 den = jumpEndPosition - jumpStartPosition; den.y = 0f; 
        float jumpProgress = num.magnitude / den.magnitude + Time.deltaTime / jumpDuration;
        Debug.Log(jumpProgress); 

        if (jumpProgress < 1f)
        {
            Vector3 jumpPosition = Vector3.Lerp(jumpStartPosition, jumpEndPosition, jumpProgress);
            jumpPosition.y += Mathf.Sin(jumpProgress * Mathf.PI) * jumpHeight;

            transform.position = jumpPosition;
        }
        else
        {
            EndJump();
        }
    }

    void EndJump()
    {
        isJumping = false;
        boidOn = true; 
    }


    void FixedUpdate()
    {
        // UpdateNeighbors();
        if (boidOn)
        {
            Vector3 separation = CalculateSeparation();
            Vector3 alignment = CalculateAlignment();
            Vector3 cohesion = CalculateCohesion();
            Vector3 mouseAttraction = CalculateMouseAttraction();

            Vector3 acceleration = separation * separationWeight +
                                   alignment * alignmentWeight +
                                   cohesion * cohesionWeight +
                                   mouseAttraction * mouseAttractionWeight;

            acceleration.y = 0;

            rb.velocity += acceleration * Time.fixedDeltaTime;
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

            Vector3 position = rb.position;
            position.y = fixedHeight;
            rb.position = position;
        }
    }

    void UpdateMousePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, new Vector3(0, fixedHeight, 0));

        if (plane.Raycast(ray, out float distance))
        {
            mousePosition = ray.GetPoint(distance);
        }
    }

    void RotateTowardsMouse()
    {
        Vector3 directionToMouse = mousePosition - transform.position;
        directionToMouse.y = 0; 

        if (directionToMouse != Vector3.zero)
        {
            float angle = Mathf.Atan2(directionToMouse.x, directionToMouse.z) * Mathf.Rad2Deg;

            angle += modelRotationOffset;

            Quaternion targetRotation = Quaternion.Euler(0, angle, 0);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    Vector3 CalculateMouseAttraction()
    {
        Vector3 toMouse = mousePosition - transform.position;
        toMouse.y = 0;
        return Vector3.ClampMagnitude(toMouse, maxSteerForce);
    }

    /*
    void UpdateNeighbors()
    {
        neighbors.Clear();
        foreach (Boid boid in FindObjectsOfType<Boid>())
        {
            if (boid != this)
            {
                Vector3 toNeighbor = boid.transform.position - transform.position;
                toNeighbor.y = 0;
                if (toNeighbor.magnitude <= neighborhoodRadius)
                {
                    neighbors.Add(boid);
                }
            }
        }
    }
    */

    Vector3 CalculateSeparation()
    {
        Vector3 separationForce = Vector3.zero;
        foreach (Boid neighbor in neighbors)
        {
            Vector3 toNeighbor = transform.position - neighbor.transform.position;
            toNeighbor.y = 0;
            separationForce += toNeighbor.normalized / toNeighbor.magnitude;
        }
        separationForce.y = 0;
        return Vector3.ClampMagnitude(separationForce, maxSteerForce);
    }

    Vector3 CalculateAlignment()
    {
        if (neighbors.Count == 0) return Vector3.zero;

        Vector3 averageVelocity = Vector3.zero;
        foreach (Boid neighbor in neighbors)
        {
            averageVelocity += neighbor.rb.velocity;
        }
        averageVelocity /= neighbors.Count;
        averageVelocity.y = 0;

        Vector3 alignmentForce = averageVelocity - rb.velocity;
        alignmentForce.y = 0;
        return Vector3.ClampMagnitude(alignmentForce, maxSteerForce);
    }

    Vector3 CalculateCohesion()
    {
        if (neighbors.Count == 0) return Vector3.zero;

        Vector3 centerOfMass = Vector3.zero;
        foreach (Boid neighbor in neighbors)
        {
            centerOfMass += neighbor.transform.position;
        }
        centerOfMass /= neighbors.Count;
        centerOfMass.y = fixedHeight;

        Vector3 cohesionForce = centerOfMass - transform.position;
        cohesionForce.y = 0;
        return Vector3.ClampMagnitude(cohesionForce, maxSteerForce);
    }
}