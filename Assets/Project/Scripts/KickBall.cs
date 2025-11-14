using UnityEngine;

public class PlayerKick : MonoBehaviour
{
    public float kickForce = 10f;             // Strength of the kick
    public float kickRange = 2f;              // Must be within this distance
    public float maxAngle = 45f;              // Player must be roughly facing the ball (in degrees)
    public KeyCode kickKey = KeyCode.Alpha0;  // Key to press (0 key)
    public Transform playerCamera;            // For forward direction (can use main camera)

    private void Update()
    {
        if (Input.GetKeyDown(kickKey))
        {
            TryKickBall();
        }
    }

    void TryKickBall()
    {
        // Find all nearby colliders in the kick range
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, kickRange);

        foreach (Collider col in nearbyObjects)
        {
            if (col.CompareTag("Ball"))
            {
                Rigidbody rb = col.attachedRigidbody;
                if (rb != null)
                {
                    Vector3 toBall = (col.transform.position - transform.position).normalized;
                    Vector3 forward = playerCamera != null ? playerCamera.forward : transform.forward;

                    // Check if player is facing the ball
                    float angle = Vector3.Angle(forward, toBall);
                    if (angle > maxAngle)
                    {
                        Debug.Log("Too far or not facing the ball.");
                        return;
                    }

                    // Apply force
                    Vector3 direction = (forward + Vector3.up * 0.3f).normalized; // add slight upward curve
                    rb.AddForce(direction * kickForce, ForceMode.Impulse);
                    Debug.Log("Kicked the ball!");
                    return;
                }
            }
        }

        Debug.Log("No ball nearby to kick.");
    }

    // Draw kick range in the Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, kickRange);
    }
}
