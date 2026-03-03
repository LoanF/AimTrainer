using System.Collections;
using UnityEngine;

public class Target : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float resetDelay = 2f;

    private Renderer targetRenderer;
    private Color originalColor;

    private void Awake()
    {
        targetRenderer = GetComponent<Renderer>();
        originalColor = targetRenderer.material.color;
    }

    public void OnBulletHit()
    {
        Debug.Log("[Target] HIT! " + gameObject.name);
        targetRenderer.material.color = hitColor;

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
