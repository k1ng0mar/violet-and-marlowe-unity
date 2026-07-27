using UnityEngine;

/// <summary>
/// A carryable item (e.g. carrot sack). When picked up, attaches as a
/// kinematic child of the player. When dropped, detaches and stays in place.
/// No physics during carry (per Carrot Crates doc).
/// </summary>
[RequireComponent(typeof(Renderer))]
public class CarryItem : MonoBehaviour
{
    [Header("Carry Settings")]
    public Vector3 carryOffset = new Vector3(0, 1.5f, 0.5f);
    public float pickupRange = 2f;

    private Transform originalParent;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Rigidbody rb;
    private bool isCarried = false;

    void Awake()
    {
        originalParent = transform.parent;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    public void PickUp(Transform carrier)
    {
        if (isCarried) return;

        isCarried = true;
        transform.SetParent(carrier, true);
        transform.localPosition = carryOffset;
        transform.localRotation = Quaternion.identity;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        Debug.Log($"[CarryItem] Picked up by {carrier.name}");
    }

    public void Drop()
    {
        if (!isCarried) return;

        isCarried = false;
        transform.SetParent(originalParent, true);
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        Debug.Log("[CarryItem] Dropped");
    }

    public bool IsCarried => isCarried;

    void OnTriggerEnter(Collider other)
    {
        // Allow pickup via trigger if player is nearby
        if (isCarried) return;
        var player = other.GetComponent<PlayerController>();
        if (player != null && Vector3.Distance(transform.position, player.transform.position) <= pickupRange)
        {
            PickUp(player.transform);
        }
    }
}