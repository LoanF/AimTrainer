using UnityEngine;

public class RoomSetup : MonoBehaviour
{
    [Header("Room Size")]
    [SerializeField] private float width = 60f;
    [SerializeField] private float length = 80f;
    [SerializeField] private float height = 25f;

    private void Awake()
    {
        // Dark indoor shooting range palette
        Color floorColor = new Color(0.12f, 0.12f, 0.14f);       // dark concrete
        Color ceilingColor = new Color(0.08f, 0.08f, 0.10f);     // near black
        Color wallColor = new Color(0.15f, 0.15f, 0.18f);        // dark grey
        Color accentColor = new Color(0.90f, 0.55f, 0.05f);      // orange accent
        Color laneColor = new Color(0.18f, 0.18f, 0.22f);        // lane dividers
        Color targetWallColor = new Color(0.10f, 0.10f, 0.10f);  // back wall (target area)

        // Room structure
        CreateSurface("Floor", Vector3.zero, new Vector3(width, 0.1f, length), floorColor);
        CreateSurface("Ceiling", new Vector3(0f, height, 0f), new Vector3(width, 0.1f, length), ceilingColor);
        CreateSurface("WallBack", new Vector3(0f, height / 2f, -length / 2f), new Vector3(width, height, 0.2f), wallColor);
        CreateSurface("TargetWall", new Vector3(0f, height / 2f, length / 2f), new Vector3(width, height, 0.3f), targetWallColor);
        CreateSurface("WallLeft", new Vector3(-width / 2f, height / 2f, 0f), new Vector3(0.2f, height, length), wallColor);
        CreateSurface("WallRight", new Vector3(width / 2f, height / 2f, 0f), new Vector3(0.2f, height, length), wallColor);

        // Floor accent stripes (shooting lanes)
        float laneWidth = 3f;
        int laneCount = 5;
        float startX = -(laneCount - 1) * laneWidth;
        for (int i = 0; i < laneCount; i++)
        {
            float x = startX + i * laneWidth * 2f;
            CreateSurface("Lane_" + i, new Vector3(x, 0.06f, 5f), new Vector3(laneWidth, 0.02f, length * 0.7f), laneColor);

            // Orange firing line per lane
            CreateSurface("FireLine_" + i, new Vector3(x, 0.07f, -length * 0.15f), new Vector3(laneWidth, 0.02f, 0.15f), accentColor);
        }

        // Warning stripes on target wall (orange accents)
        CreateSurface("AccentTop", new Vector3(0f, height - 0.3f, length / 2f - 0.1f), new Vector3(width, 0.6f, 0.05f), accentColor);
        CreateSurface("AccentBottom", new Vector3(0f, 0.3f, length / 2f - 0.1f), new Vector3(width, 0.6f, 0.05f), accentColor);

        // Ceiling lights (emissive strips)
        for (int i = 0; i < 6; i++)
        {
            float z = -length * 0.35f + i * (length * 0.7f / 5f);
            var light = CreateSurface("Light_" + i, new Vector3(0f, height - 0.08f, z), new Vector3(width * 0.6f, 0.05f, 0.4f), Color.white);
            SetEmissive(light, Color.white, 3f);
        }

        // Side accent lights
        for (int i = 0; i < 4; i++)
        {
            float z = -length * 0.3f + i * (length * 0.6f / 3f);
            var lightL = CreateSurface("SideLight_L_" + i, new Vector3(-width / 2f + 0.15f, height * 0.7f, z), new Vector3(0.05f, 0.3f, 1.5f), accentColor);
            var lightR = CreateSurface("SideLight_R_" + i, new Vector3(width / 2f - 0.15f, height * 0.7f, z), new Vector3(0.05f, 0.3f, 1.5f), accentColor);
            SetEmissive(lightL, accentColor, 2f);
            SetEmissive(lightR, accentColor, 2f);
        }
    }

    private GameObject CreateSurface(string name, Vector3 position, Vector3 scale, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(transform);
        go.transform.localPosition = position;
        go.transform.localScale = scale;

        var renderer = go.GetComponent<Renderer>();
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.SetColor("_BaseColor", color);
        renderer.material = mat;
        return go;
    }

    private void SetEmissive(GameObject go, Color color, float intensity)
    {
        var renderer = go.GetComponent<Renderer>();
        var mat = renderer.material;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color * intensity);
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
    }
}
