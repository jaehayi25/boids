using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class BoidNavMeshAgent : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float neighborRadius = 5f;
    public float separationWeight = 1f;
    public float alignmentWeight = 1f;
    public float cohesionWeight = 1f;
    public float updateInterval = 0.1f;

    private NavMeshAgent agent;
    private List<BoidNavMeshAgent> neighbors = new List<BoidNavMeshAgent>();
    private float timer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        timer = updateInterval;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            UpdateBoidBehavior();
            timer = updateInterval;
        }
    }

    void UpdateBoidBehavior()
    {
        FindNeighbors();
        Vector3 separation = CalculateSeparation();
        Vector3 alignment = CalculateAlignment();
        Vector3 cohesion = CalculateCohesion();

        Vector3 movement = separation * separationWeight +
                           alignment * alignmentWeight +
                           cohesion * cohesionWeight;

        Vector3 targetPosition = transform.position + movement;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, 1.0f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void FindNeighbors()
    {
        neighbors.Clear();
        BoidNavMeshAgent[] allBoids = FindObjectsOfType<BoidNavMeshAgent>();
        foreach (BoidNavMeshAgent boid in allBoids)
        {
            if (boid != this && Vector3.Distance(transform.position, boid.transform.position) <= neighborRadius)
            {
                neighbors.Add(boid);
            }
        }
    }

    Vector3 CalculateSeparation()
    {
        Vector3 separation = Vector3.zero;
        foreach (BoidNavMeshAgent neighbor in neighbors)
        {
            Vector3 diff = transform.position - neighbor.transform.position;
            separation += diff.normalized / diff.magnitude;
        }
        return separation.normalized;
    }

    Vector3 CalculateAlignment()
    {
        if (neighbors.Count == 0) return Vector3.zero;

        Vector3 averageVelocity = Vector3.zero;
        foreach (BoidNavMeshAgent neighbor in neighbors)
        {
            averageVelocity += neighbor.agent.velocity;
        }
        averageVelocity /= neighbors.Count;
        return averageVelocity.normalized;
    }

    Vector3 CalculateCohesion()
    {
        if (neighbors.Count == 0) return Vector3.zero;

        Vector3 centerOfMass = Vector3.zero;
        foreach (BoidNavMeshAgent neighbor in neighbors)
        {
            centerOfMass += neighbor.transform.position;
        }
        centerOfMass /= neighbors.Count;

        return (centerOfMass - transform.position).normalized;
    }
}