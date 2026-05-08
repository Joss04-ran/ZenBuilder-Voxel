using UnityEngine;
using System;

[Serializable]
public class NoiseSettings
{
    [Tooltip("Overall zoom level — higher value = more zoomed out terrain")]
    public float Scale = 100f;

    [Tooltip("How many noise layers to stack — more octaves = more detail")]
    [Range(1, 8)]
    public int Octaves = 4;

    [Tooltip("How much each octave contributes — lower = smoother")]
    [Range(0f, 1f)]
    public float Persistence = 0.5f;

    [Tooltip("How much frequency increases per octave — higher = more detail")]
    [Range(1f, 4f)]
    public float Lacunarity = 2f;

    [Tooltip("Offset to produce unique worlds — change this for a different seed")]
    public float Seed = 0f;
    public NoiseSettings(float scale = 100f, int octaves = 4,
        float persistence = 0.5f, float lacunarity = 2f, float seed = 0f)
    {
        Scale = scale;
        Octaves = octaves;
        Persistence = persistence;
        Lacunarity = lacunarity;
        Seed = seed;
    }
}