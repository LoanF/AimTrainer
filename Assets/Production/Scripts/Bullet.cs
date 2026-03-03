using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 5f;

    private Vector3 previousPosition;

    private void Awake()
    {
        SetupVisuals();
        SetupTrail();
    }

    private void Start()
    {
        previousPosition = transform.position;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;

        Vector3 direction = transform.position - previousPosition;
        float distance = direction.magnitude;

        if (distance > 0f && Physics.Raycast(previousPosition, direction.normalized, out RaycastHit hit, distance))
        {
            hit.collider.GetComponent<Target>()?.OnBulletHit();
            Destroy(gameObject);
            return;
        }

        previousPosition = transform.position;
    }

    private void SetupVisuals()
    {
        var renderer = GetComponent<Renderer>();
        if (renderer == null) return;

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        Color gold = new Color(0.83f, 0.69f, 0.22f);
        mat.SetColor("_BaseColor", gold);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", gold * 1.5f);
        mat.SetFloat("_Metallic", 0.9f);
        mat.SetFloat("_Smoothness", 0.8f);
        renderer.material = mat;
    }

    private void SetupTrail()
    {
        var trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = 0.3f;
        trail.startWidth = 0.02f;
        trail.endWidth = 0f;
        trail.material = new Material(Shader.Find("Sprites/Default"));
        Color gold = new Color(0.83f, 0.69f, 0.22f);
        trail.startColor = gold;
        trail.endColor = new Color(gold.r, gold.g, gold.b, 0f);
    }
}
