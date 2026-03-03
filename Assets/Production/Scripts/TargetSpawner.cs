using UnityEngine;

public class TargetSpawner : MonoBehaviour
{
    [Header("Spawn Zone")]
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;
    [SerializeField] private float minY = 1f;
    [SerializeField] private float maxY = 6f;
    [SerializeField] private float minZ = 5f;
    [SerializeField] private float maxZ = 15f;

    [Header("Target")]
    [SerializeField] private Vector3 targetScale = Vector3.one;

    private GameObject targetObject;
    private Target target;

    private void Awake()
    {
        targetObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        targetObject.name = "Target";
        targetObject.transform.localScale = targetScale;

        var renderer = targetObject.GetComponent<Renderer>();
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.SetColor("_BaseColor", new Color(0.9f, 0.2f, 0.1f));
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(0.9f, 0.2f, 0.1f) * 0.5f);
        renderer.material = mat;

        target = targetObject.AddComponent<Target>();
        target.OnHit += SpawnAtRandomPosition;

        SpawnAtRandomPosition();
    }

    private void SpawnAtRandomPosition()
    {
        float x = Random.Range(minX, maxX);
        float y = Random.Range(minY, maxY);
        float z = Random.Range(minZ, maxZ);
        targetObject.transform.position = new Vector3(x, y, z);
    }
}
