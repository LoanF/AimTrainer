using System.Collections;
using UnityEngine;

public class Target : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float resetDelay = 2f;

    private Renderer targetRenderer;
    private Color originalColor;
    private AudioSource audioSource;
    private AudioClip hitClip;

    private void Awake()
    {
        targetRenderer = GetComponent<Renderer>();
        originalColor = targetRenderer.material.color;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialize = true;
        hitClip = SoundGenerator.GenerateHitSound();
    }

    public void OnBulletHit()
    {
        targetRenderer.material.color = hitColor;
        audioSource.PlayOneShot(hitClip);

        if (resetDelay > 0f)
        {
            StopAllCoroutines();
            StartCoroutine(ResetColor());
        }
    }

    private IEnumerator ResetColor()
    {
        yield return new WaitForSeconds(resetDelay);
        targetRenderer.material.color = originalColor;
    }
}
