using UnityEngine;

public class FishMovement : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float rotationSpeed = 2f;
    public float changeDirectionInterval = 3f;
    public float maxRotationAngle = 30f;
    public Vector3 modelRotationOffset = new Vector3(0, 90, 0); // Adjust this based on your model's orientation

    private Vector3 targetDirection;
    private Quaternion targetRotation;
    private float timeSinceLastDirectionChange;

    private void Start()
    {
        SetNewTargetDirection();
    }

    private void Update()
    {
        // Smoothly rotate towards the target direction
        Quaternion currentRotation = transform.rotation * Quaternion.Euler(modelRotationOffset);
        Quaternion smoothRotation = Quaternion.Slerp(currentRotation, targetRotation, rotationSpeed * Time.deltaTime);
        transform.rotation = smoothRotation * Quaternion.Inverse(Quaternion.Euler(modelRotationOffset));

        // Move forward in the direction the fish is facing
        transform.Translate(transform.right * moveSpeed * Time.deltaTime);

        // Check if it's time to change direction
        timeSinceLastDirectionChange += Time.deltaTime;
        if (timeSinceLastDirectionChange >= changeDirectionInterval)
        {
            SetNewTargetDirection();
            timeSinceLastDirectionChange = 0f;
        }
    }

    private void SetNewTargetDirection()
    {
        // Generate a random direction in the XZ plane
        float randomAngle = Random.Range(0f, 360f);
        targetDirection = Quaternion.Euler(0f, randomAngle, 0f) * Vector3.forward;

        // Calculate the target rotation, including the model's rotation offset
        targetRotation = Quaternion.LookRotation(targetDirection) * Quaternion.Euler(modelRotationOffset);

        // Add some random rotation around the Y-axis for natural movement
        float randomYRotation = Random.Range(-maxRotationAngle, maxRotationAngle);
        targetRotation *= Quaternion.Euler(0f, randomYRotation, 0f);
    }
}