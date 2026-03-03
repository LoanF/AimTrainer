using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 5f;

    private Vector3 previousPosition;

    private void Awake()
    {
        SetupTrail();
    }

    private void Start()
    {
        previousPosition = transform.position;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Move forward
        transform.position += transform.forward * speed * Time.deltaTime;

        // Raycast between previous and current position to detect hits
        Vector3 direction = transform.position - previousPosition;
        float distance = direction.magnitude;

        if (distance > 0f && Physics.Raycast(previousPosition, direction.normalized, out RaycastHit hit, distance))
        {
            Debug.Log("[Bullet] Hit: " + hit.collider.gameObject.name);

            var target = hit.collider.GetComponent<Target>();
            if (target != null)
            {
                target.OnBulletHit();
            }

            Destroy(gameObject);
            return;
        }

        previousPosition = transform.position;
    }

    private void SetupTrail()
    {
        var trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = 0.3f;
        trail.startWidth = 0.02f;
        trail.endWidth = 0f;
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.startColor = Color.yellow;
        trail.endColor = new Color(1f, 1f, 0f, 0f);
    }
}
