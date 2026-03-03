using UnityEngine;

public static class SoundGenerator
{
    private const int SampleRate = 44100;

    public static AudioClip GenerateShootSound()
    {
        int samples = SampleRate / 5; // 0.2s
        var clip = AudioClip.Create("Shoot", samples, 1, SampleRate, false);
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float freq = Mathf.Lerp(800f, 200f, t); // descending pitch
            float wave = Mathf.Sin(2f * Mathf.PI * freq * t);
            float noise = (Random.value * 2f - 1f) * 0.3f;
            float envelope = 1f - t; // fade out
            data[i] = (wave * 0.7f + noise) * envelope * 0.5f;
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
