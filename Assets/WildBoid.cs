using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildBoid : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CheckForBoids(); 
    }

    private void CheckForBoids()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 10f); 
        foreach (var collider in colliders)
        {
            if (collider.GetComponent<Boid>() != null)
            {
                BoidManager.Instance.AddBoid(gameObject);
                Destroy(GetComponent<WildBoid>());
                return; 
            }
        }

    }
}
