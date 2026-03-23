using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed = 60f;
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
        // Redimensionne en petit projectile réaliste (~9mm)
        transform.localScale = new Vector3(0.008f, 0.008f, 0.018f);

        var renderer = GetComponent<Renderer>();
        if (renderer == null) return;

        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        // Couleur plomb/cuivre sombre
        mat.SetColor("_BaseColor", new Color(0.25f, 0.18f, 0.10f));
        mat.SetFloat("_Metallic", 0.85f);
        mat.SetFloat("_Smoothness", 0.4f);
        renderer.material = mat;
    }

    private void SetupTrail()
    {
        var trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = 0.06f;
        trail.startWidth = 0.004f;
        trail.endWidth = 0f;
        trail.material = new Material(Shader.Find("Sprites/Default"));
        // Trainée de fumée blanche/grise semi-transparente
        trail.startColor = new Color(0.85f, 0.85f, 0.85f, 0.5f);
        trail.endColor = new Color(0.6f, 0.6f, 0.6f, 0f);
    }
}
