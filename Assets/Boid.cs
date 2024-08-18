using UnityEngine;
using UnityEngine.AI;
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

    [HideInInspector]
    public List<Boid> neighbors = new List<Boid>();

    private Rigidbody rb;
    private static Vector3 mousePosition;

    private NavMeshAgent agent;

    public float jumpHeight = 2f;
    bool isJumping = false;
    private Vector3 jumpStartPosition;
    private Vector3 jumpEndPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();

        Vector3 position = transform.position;
        position.y = fixedHeight;
        transform.position = position;
    }

    void Update()
    {
        UpdateMousePosition();
        RotateTowardsMouse();
        JumpIfOnLink(); 
    }

    void FixedUpdate()
    {
        // UpdateNeighbors();
        if (!isJumping)
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

    void JumpIfOnLink()
    {
        Debug.Log(agent.isOnOffMeshLink); 
        if (agent.isOnOffMeshLink)
        {
            if (!isJumping)
            {
                StartJump();
            }
            else
            {
                UpdateJump();
            }
        }
        else
        {
            isJumping = false;
        }
    }

    void StartJump()
    {
        isJumping = true;
        agent.updatePosition = false;
        agent.updateRotation = false;

        OffMeshLinkData data = agent.currentOffMeshLinkData;
        jumpStartPosition = transform.position;
        jumpEndPosition = data.endPos + Vector3.up * agent.baseOffset;

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
        float jumpProgress = (transform.position - jumpStartPosition).magnitude / (jumpEndPosition - jumpStartPosition).magnitude;

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
        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.CompleteOffMeshLink();
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

    Vector3 CalculateMouseAttraction()
    {
        Vector3 toMouse = mousePosition - transform.position;
        toMouse.y = 0;
        return Vector3.ClampMagnitude(toMouse, maxSteerForce);
    }

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