using UnityEngine;
using System.Collections.Generic;

public class BoidManager : MonoBehaviour
{
    public static BoidManager Instance; 

    public float neighborhoodRadius = 5f;

    [SerializeField]
    private List<Boid> boids = new List<Boid>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {

    }

    void FixedUpdate()
    {

        UpdateAllNeighborsPeriodicaly(); 
    }

    public void AddBoid(GameObject newBoidGO)
    {
        // Vector3 randomPosition = transform.position + Random.insideUnitSphere * spawnRadius;
        // randomPosition.y = 0; // Assuming we're keeping boids on a plane

        // GameObject boidObject = Instantiate(boidPrefab, randomPosition, Quaternion.identity);

        Boid newBoid = newBoidGO.AddComponent(typeof(Boid)) as Boid; 

        if (newBoid != null)
        {
            boids.Add(newBoid);
            UpdateAllNeighbors();
        }
        else
        {
            Debug.LogError("Boid component not found on the instantiated prefab!");
        }
    }

    private void UpdateAllNeighbors()
    {
        foreach (Boid boid in boids)
        {
            UpdateNeighborsForBoid(boid);
        }
    }

    private void UpdateNeighborsForBoid(Boid boid)
    {
        boid.neighbors.Clear();
        foreach (Boid otherBoid in boids)
        {
            if (otherBoid != boid && Vector3.Distance(boid.transform.position, otherBoid.transform.position) <= neighborhoodRadius)
            {
                boid.neighbors.Add(otherBoid);
            }
        }
    }

    // Call this method periodically to update neighbors (e.g., every few seconds or frames)
    public void UpdateAllNeighborsPeriodicaly()
    {
        UpdateAllNeighbors();
    }

    private Vector3 GetCenterOfMass()
    {
        Vector3 centerOfMass = Vector3.zero;
        foreach (Boid boid in boids)
        {
            centerOfMass += boid.transform.position;
        }
        centerOfMass /= boids.Count;
        centerOfMass.y = 0;

        return centerOfMass;
    }
}