using UnityEngine;

public static class SoundGenerator
{
    private const int SampleRate = 44100;

    public static AudioClip GenerateShootSound()
    {
        // 320ms : crack initial + boom basse + queue de réverbération
        int samples = (int)(SampleRate * 0.32f);
        var clip = AudioClip.Create("Shoot", samples, 1, SampleRate, false);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / SampleRate;

            // --- Crack supersonique (2ms, très court et fort) ---
            float crack = 0f;
            if (t < 0.002f)
                crack = (Random.value * 2f - 1f) * Mathf.Exp(-t * 1200f) * 1.8f;

            // --- Boom basse (70Hz, décroissance rapide) ---
            float bass = Mathf.Sin(2f * Mathf.PI * 70f * t) * Mathf.Exp(-t * 22f) * 0.9f;

            // --- Corps médium (descente 350→100Hz) ---
            float freqMid = 350f * Mathf.Exp(-t * 8f) + 100f;
            float mid = Mathf.Sin(2f * Mathf.PI * freqMid * t) * Mathf.Exp(-t * 18f) * 0.5f;

            // --- Bruit d'explosion (gravier/charge) ---
            float noise = (Random.value * 2f - 1f) * Mathf.Exp(-t * 35f) * 0.7f;

            // --- Queue basse (résonance de chambre) ---
            float tail = Mathf.Sin(2f * Mathf.PI * 55f * t) * Mathf.Exp(-t * 9f) * 0.25f;

            float sample = crack + bass + mid + noise + tail;
            data[i] = Mathf.Clamp(sample * 0.55f, -1f, 1f);
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
