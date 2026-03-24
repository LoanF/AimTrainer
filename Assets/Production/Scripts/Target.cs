using System;
using UnityEngine;

public class Target : MonoBehaviour
{
    public Action OnHit;

    private AudioSource audioSource;
    private AudioClip hitClip;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialize = true;
        hitClip = Resources.Load<AudioClip>("cible") ?? SoundGenerator.GenerateHitSound();
    }

    public void SetHitSound(AudioClip clip)
    {
        if (clip != null) hitClip = clip;
    }

    public void OnBulletHit()
    {
        audioSource.PlayOneShot(hitClip);
        OnHit?.Invoke();
    }
}
