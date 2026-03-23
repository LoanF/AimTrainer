using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class WeaponPickup : MonoBehaviour
{
    [Header("Weapon Config")]
    public WeaponData weaponData;

    [Header("Muzzle Point")]
    [Tooltip("Child transform placed at the muzzle of the gun")]
    public Transform muzzlePoint;

    [Header("Two-Handed")]
    [Tooltip("Child transform placed under the barrel (foregrip position)")]
    public Transform secondaryGripPoint;

    [Header("Hold Offset (tuned per model)")]
    public Vector3 holdPositionOffset = Vector3.zero;
    public Vector3 holdRotationOffset = Vector3.zero;

    [Header("Float Animation")]
    [SerializeField] private float bobAmplitude = 0.06f;
    [SerializeField] private float bobSpeed = 1.5f;
    [SerializeField] private float rotationSpeed = 50f;

    private Vector3 startPosition;
    private bool isPickedUp;
    private Rigidbody rb;
    private Collider col;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        col = GetComponent<Collider>();
        col.isTrigger = true; // trigger = pas de collision physique avec le joueur

        startPosition = transform.position;
    }

    private void Update()
    {
        if (isPickedUp) return;

        float y = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        transform.position = new Vector3(startPosition.x, y, startPosition.z);
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }

    public void OnPickup(Transform socket)
    {
        isPickedUp = true;
        col.enabled = false;

        transform.SetParent(socket);
        transform.localPosition = holdPositionOffset;
        transform.localRotation = Quaternion.Euler(holdRotationOffset);
    }

    public void OnDrop(Vector3 dropPosition)
    {
        transform.SetParent(null);
        transform.position = dropPosition;
        transform.rotation = Quaternion.identity;
        startPosition = dropPosition;

        col.enabled = true;
        isPickedUp = false;
    }
}
