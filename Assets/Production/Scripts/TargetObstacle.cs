using UnityEngine;

public class TargetObstacle : MonoBehaviour
{
    [HideInInspector]
    public TargetManager manager;

    [Header("Target Properties")]
    public float speed = 0f;
    public Color targetColor = Color.white;

    [Header("Sound")]
    [SerializeField] private AudioClip hitSoundOverride;

    private AudioSource audioSource;

    private Vector3 startPos;
    private float randOffset;
    private Material instantiatedMaterial;

    void Start()
    {
        startPos = transform.position;
        randOffset = Random.Range(0f, 100f);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialize = true;

        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            instantiatedMaterial = rend.material;
            instantiatedMaterial.color = targetColor;
        }
    }

    void Update()
    {
        if (speed > 0)
        {
            float move = Mathf.Sin(Time.time * speed + randOffset) * 2f;
            transform.position = startPos + transform.right * move;
        }
    }

    void OnDestroy()
    {
        if (instantiatedMaterial != null)
        {
            Destroy(instantiatedMaterial);
        }
    }

    [ContextMenu("Force WasShot")]
    public void WasShot()
    {
        AudioClip clip = hitSoundOverride != null ? hitSoundOverride : Resources.Load<AudioClip>("cible") ?? SoundGenerator.GenerateHitSound();
        AudioSource.PlayClipAtPoint(clip, transform.position);

        if (manager != null)
        {
            manager.RelocateTarget(this.transform.position);
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("This Target has no Manager assigned! Check your Spawner logic.");
        }
    }
}