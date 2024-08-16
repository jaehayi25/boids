using UnityEngine;
using System.Collections.Generic;

public class CameraFollowAndZoom : MonoBehaviour
{
    private List<Transform> targets; // List of objects to frame
    public Vector3 offset = new Vector3(0, 10, -10); // Camera offset
    public float smoothTime = 0.5f; // Smoothing for camera movement
    public float minZoom = 40f; // Minimum FOV
    public float maxZoom = 10f; // Maximum FOV
    public float zoomLimiter = 50f; // Limits how far the camera zooms out

    private Vector3 velocity;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        targets = BoidManager.Instance.getBoidTransforms(); 

        if (targets.Count == 0)
            return;

        Move();
        Zoom();
    }

    void Move()
    {
        Vector3 centerPoint = GetCenterPoint();
        Vector3 newPosition = centerPoint + offset;
        transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothTime);
    }

    void Zoom()
    {
        float newZoom = Mathf.Lerp(maxZoom, minZoom, GetGreatestDistance() / zoomLimiter);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, newZoom, Time.deltaTime);
    }

    float GetGreatestDistance()
    {
        var bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }

        return bounds.size.x > bounds.size.z ? bounds.size.x : bounds.size.z;
    }

    Vector3 GetCenterPoint()
    {
        if (targets.Count == 1)
        {
            return targets[0].position;
        }

        var bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }

        return bounds.center;
    }
}