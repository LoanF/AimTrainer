using UnityEngine;

public static class SoundGenerator
{
    private const int SampleRate = 44100;

    public enum ProceduralSound { Pistol, Rifle, Shotgun, SMG, Sniper }

    public static AudioClip GenerateShootSound(ProceduralSound profile = ProceduralSound.Pistol)
    {
        return profile switch
        {
            ProceduralSound.Rifle   => GenerateRifleSound(),
            ProceduralSound.Shotgun => GenerateShotgunSound(),
            ProceduralSound.SMG     => GenerateSMGSound(),
            ProceduralSound.Sniper  => GenerateSniperSound(),
            _                       => GeneratePistolSound(),
        };
    }

    // --- Pistol : crack sec, boom court (320ms) ---
    private static AudioClip GeneratePistolSound()
    {
        int samples = (int)(SampleRate * 0.32f);
        var clip = AudioClip.Create("Shoot_Pistol", samples, 1, SampleRate, false);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float crack = t < 0.002f ? (Random.value * 2f - 1f) * Mathf.Exp(-t * 1200f) * 1.8f : 0f;
            float bass  = Mathf.Sin(2f * Mathf.PI * 70f * t)  * Mathf.Exp(-t * 22f) * 0.9f;
            float freqMid = 350f * Mathf.Exp(-t * 8f) + 100f;
            float mid   = Mathf.Sin(2f * Mathf.PI * freqMid * t) * Mathf.Exp(-t * 18f) * 0.5f;
            float noise = (Random.value * 2f - 1f) * Mathf.Exp(-t * 35f) * 0.7f;
            float tail  = Mathf.Sin(2f * Mathf.PI * 55f * t)  * Mathf.Exp(-t * 9f)  * 0.25f;
            data[i] = Mathf.Clamp((crack + bass + mid + noise + tail) * 0.55f, -1f, 1f);
        }
        clip.SetData(data, 0);
        return clip;
    }

    // --- Rifle : plus fort, corps médium plus présent, queue plus longue (420ms) ---
    private static AudioClip GenerateRifleSound()
    {
        int samples = (int)(SampleRate * 0.42f);
        var clip = AudioClip.Create("Shoot_Rifle", samples, 1, SampleRate, false);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float crack = t < 0.003f ? (Random.value * 2f - 1f) * Mathf.Exp(-t * 900f) * 2.0f : 0f;
            float bass  = Mathf.Sin(2f * Mathf.PI * 80f * t)  * Mathf.Exp(-t * 15f) * 1.1f;
            float freqMid = 400f * Mathf.Exp(-t * 6f) + 120f;
            float mid   = Mathf.Sin(2f * Mathf.PI * freqMid * t) * Mathf.Exp(-t * 12f) * 0.8f;
            float noise = (Random.value * 2f - 1f) * Mathf.Exp(-t * 25f) * 0.9f;
            float tail  = Mathf.Sin(2f * Mathf.PI * 50f * t)  * Mathf.Exp(-t * 6f)  * 0.4f;
            data[i] = Mathf.Clamp((crack + bass + mid + noise + tail) * 0.5f, -1f, 1f);
        }
        clip.SetData(data, 0);
        return clip;
    }

    // --- Shotgun : boom très grave et large, très court et percutant (280ms) ---
    private static AudioClip GenerateShotgunSound()
    {
        int samples = (int)(SampleRate * 0.28f);
        var clip = AudioClip.Create("Shoot_Shotgun", samples, 1, SampleRate, false);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float crack = t < 0.004f ? (Random.value * 2f - 1f) * Mathf.Exp(-t * 700f) * 2.5f : 0f;
            float bass  = Mathf.Sin(2f * Mathf.PI * 45f * t)  * Mathf.Exp(-t * 12f) * 1.5f;
            float bass2 = Mathf.Sin(2f * Mathf.PI * 90f * t)  * Mathf.Exp(-t * 18f) * 0.8f;
            float noise = (Random.value * 2f - 1f) * Mathf.Exp(-t * 20f) * 1.2f;
            float tail  = Mathf.Sin(2f * Mathf.PI * 40f * t)  * Mathf.Exp(-t * 7f)  * 0.5f;
            data[i] = Mathf.Clamp((crack + bass + bass2 + noise + tail) * 0.45f, -1f, 1f);
        }
        clip.SetData(data, 0);
        return clip;
    }

    // --- SMG : léger, sec, rapide (220ms) ---
    private static AudioClip GenerateSMGSound()
    {
        int samples = (int)(SampleRate * 0.22f);
        var clip = AudioClip.Create("Shoot_SMG", samples, 1, SampleRate, false);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float crack = t < 0.0015f ? (Random.value * 2f - 1f) * Mathf.Exp(-t * 1500f) * 1.4f : 0f;
            float bass  = Mathf.Sin(2f * Mathf.PI * 100f * t) * Mathf.Exp(-t * 30f) * 0.6f;
            float freqMid = 300f * Mathf.Exp(-t * 12f) + 90f;
            float mid   = Mathf.Sin(2f * Mathf.PI * freqMid * t) * Mathf.Exp(-t * 25f) * 0.35f;
            float noise = (Random.value * 2f - 1f) * Mathf.Exp(-t * 50f) * 0.5f;
            data[i] = Mathf.Clamp((crack + bass + mid + noise) * 0.6f, -1f, 1f);
        }
        clip.SetData(data, 0);
        return clip;
    }

    // --- Sniper : crack très net, longue réverbération (600ms) ---
    private static AudioClip GenerateSniperSound()
    {
        int samples = (int)(SampleRate * 0.60f);
        var clip = AudioClip.Create("Shoot_Sniper", samples, 1, SampleRate, false);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;
            float crack = t < 0.002f ? (Random.value * 2f - 1f) * Mathf.Exp(-t * 1000f) * 2.2f : 0f;
            float bass  = Mathf.Sin(2f * Mathf.PI * 60f * t)  * Mathf.Exp(-t * 10f) * 1.0f;
            float freqMid = 500f * Mathf.Exp(-t * 5f) + 80f;
            float mid   = Mathf.Sin(2f * Mathf.PI * freqMid * t) * Mathf.Exp(-t * 8f)  * 0.6f;
            float noise = (Random.value * 2f - 1f) * Mathf.Exp(-t * 20f) * 0.6f;
            float tail  = Mathf.Sin(2f * Mathf.PI * 45f * t)  * Mathf.Exp(-t * 4f)  * 0.35f;
            data[i] = Mathf.Clamp((crack + bass + mid + noise + tail) * 0.48f, -1f, 1f);
        }
        clip.SetData(data, 0);
        return clip;
    }

    public static AudioClip GenerateHitSound()
    {
        int samples = SampleRate / 4; // 0.25s
        var clip = AudioClip.Create("Hit", samples, 1, SampleRate, false);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float wave = Mathf.Sin(2f * Mathf.PI * 440f * t) + Mathf.Sin(2f * Mathf.PI * 880f * t) * 0.5f;
            float envelope = 1f - t;
            data[i] = wave * envelope * 0.4f;
        }
        clip.SetData(data, 0);
        return clip;
    }
}
