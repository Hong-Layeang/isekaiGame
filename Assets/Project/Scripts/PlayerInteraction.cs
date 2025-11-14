using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Sword Swing Settings")]
    public float swingAngle = 60f;       // how far the sword swings
    public float swingSpeed = 10f;       // how fast it swings
    public Transform swordTransform;     // assign your sword object in the inspector
    private bool isSwinging = false;
    private float swingProgress = 0f;
    private Quaternion originalRotation;

    [Header("Sword Attack Settings")]
    public float attackRange = 2f;
    public float attackForce = 10f; 
    public LayerMask attackLayers;   // choose what the sword can hit

    [Header("Ball Throw Settings")]
    public float throwForce = 15f;
    public float ballPickUpRange = 3f;

    [Header("Kick Settings")]
    public float kickForce = 10f;
    public float kickRange = 2f;
    public float maxAngle = 45f;
    public KeyCode kickKey = KeyCode.F;  // Separate key for kicking

    [Header("Pickup Settings")]
    public float pickUpRange = 3f;
    public Transform holdPoint;

    [Header("Key")]
    public KeyCode interactKey = KeyCode.E;

    public Transform playerCamera;

    private Rigidbody heldObject;
    private bool isHoldingBall = false;

    private void Start()
    {
        // Store the original rotation of the sword
        if (swordTransform != null)
        {
            originalRotation = swordTransform.localRotation;
        }
    }

    private void Update()
    {
        // Handle sword swing animation
        if (isSwinging)
        {
            UpdateSwordSwing();
        }

        if (Input.GetKeyDown(interactKey))
        {
            // If holding a ball → Shoot it
            if (isHoldingBall && heldObject != null)
            {
                ShootBall();
                return;
            }

            // If holding something else → Drop it
            if (heldObject != null)
            {
                DropObject();
                return;
            }

            // Try pick up a ball first
            if (TryPickUpBall())
                return;

            // Try pick up other objects
            if (TryPickUp())
                return;
        }

        // F key to kick ball (without picking it up)
        if (Input.GetKeyDown(kickKey))
        {
            TryKickBall();
        }

        // LEFT CLICK SWORD ATTACK
        if (heldObject != null && Input.GetMouseButtonDown(0))
        {
            if (!isSwinging) // Only start new swing if not already swinging
            {
                StartSwordSwing();
            }
        }
    }

    // -----------------------------------------------------------
    // PICK UP SYSTEM
    // -----------------------------------------------------------
    bool TryPickUp()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickUpRange))
        {
            if (hit.collider.CompareTag("PickUp"))
            {
                Rigidbody rb = hit.collider.attachedRigidbody;
                if (rb != null)
                {
                    heldObject = rb;
                    heldObject.detectCollisions = false;
                    heldObject.useGravity = false;
                    heldObject.isKinematic = true;

                    heldObject.transform.SetParent(holdPoint, true);
                    heldObject.transform.localPosition = Vector3.zero;
                    heldObject.transform.localRotation = Quaternion.identity;

                    // Store the original rotation when picking up
                    swordTransform = heldObject.transform;
                    originalRotation = swordTransform.localRotation;
                    isHoldingBall = false;

                    return true;
                }
            }
        }
        return false;
    }

    void DropObject()
    {
        heldObject.detectCollisions = true;
        heldObject.useGravity = true;
        heldObject.isKinematic = false;

        heldObject.transform.SetParent(null, true);

        heldObject = null;
        swordTransform = null;
        isSwinging = false;
        swingProgress = 0f;
        isHoldingBall = false;
    }

    // -----------------------------------------------------------
    // BALL PICK UP & SHOOT SYSTEM
    // -----------------------------------------------------------
    bool TryPickUpBall()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, ballPickUpRange))
        {
            if (hit.collider.CompareTag("Ball"))
            {
                Rigidbody rb = hit.collider.attachedRigidbody;
                if (rb != null)
                {
                    heldObject = rb;
                    heldObject.detectCollisions = false;
                    heldObject.useGravity = false;
                    heldObject.isKinematic = true;

                    heldObject.transform.SetParent(holdPoint, true);
                    heldObject.transform.localPosition = Vector3.zero;
                    heldObject.transform.localRotation = Quaternion.identity;

                    isHoldingBall = true;
                    Debug.Log("Picked up ball! Press E to shoot.");

                    return true;
                }
            }
        }
        return false;
    }

    void ShootBall()
    {
        if (heldObject == null) return;

        heldObject.detectCollisions = true;
        heldObject.useGravity = true;
        heldObject.isKinematic = false;
        heldObject.transform.SetParent(null, true);

        // Shoot the ball forward
        Vector3 shootDirection = (playerCamera.forward + Vector3.up * 0.2f).normalized;
        heldObject.AddForce(shootDirection * throwForce, ForceMode.Impulse);

        Debug.Log("Ball shot!");

        heldObject = null;
        isHoldingBall = false;
    }

    // -----------------------------------------------------------
    // KICK BALL SYSTEM (without picking up)
    // -----------------------------------------------------------
    void TryKickBall()
    {
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, kickRange);

        foreach (Collider col in nearbyObjects)
        {
            if (col.CompareTag("Ball"))
            {
                Rigidbody rb = col.attachedRigidbody;
                if (rb != null)
                {
                    Vector3 toBall = (col.transform.position - transform.position).normalized;
                    Vector3 forward = playerCamera.forward;

                    float angle = Vector3.Angle(forward, toBall);
                    if (angle > maxAngle)
                        continue;  // Try next ball if this one is not in front

                    Vector3 direction = (forward + Vector3.up * 0.3f).normalized;
                    rb.AddForce(direction * kickForce, ForceMode.Impulse);
                    Debug.Log("Kicked the ball!");
                    return;
                }
            }
        }
    }

    // -----------------------------------------------------------
    // SWORD SWING ANIMATION SYSTEM
    // -----------------------------------------------------------
    void StartSwordSwing()
    {
        if (swordTransform == null) return;
        
        isSwinging = true;
        swingProgress = 0f;
        SwordAttack(); // Trigger the attack when swing starts
    }

    void UpdateSwordSwing()
    {
        if (swordTransform == null)
        {
            isSwinging = false;
            return;
        }

        // Increment progress
        swingProgress += Time.deltaTime * swingSpeed;

        // Calculate swing angle using a sine wave for smooth motion
        // This creates a swing forward and back motion
        float angle = Mathf.Sin(swingProgress * Mathf.PI) * swingAngle;

        // Apply rotation around the X-axis (swings up and down)
        Quaternion swingRotation = Quaternion.Euler(-angle, 0f, 0f);
        swordTransform.localRotation = originalRotation * swingRotation;

        // End swing when progress completes one full cycle
        if (swingProgress >= 1f)
        {
            swordTransform.localRotation = originalRotation;
            isSwinging = false;
            swingProgress = 0f;
        }
    }

    // -----------------------------------------------------------
    // Sword Attack SYSTEM
    // -----------------------------------------------------------
    void SwordAttack()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, attackRange, attackLayers))
        {
            // If the object has a rigidbody → push it back
            Rigidbody rb = hit.collider.attachedRigidbody;
            if (rb != null)
            {
                rb.AddForce(playerCamera.forward * attackForce, ForceMode.Impulse);
            }

            Debug.Log("Hit with sword: " + hit.collider.name);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(playerCamera.position, playerCamera.forward * ballPickUpRange);
        }

        // Show kick range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, kickRange);
    }
}
