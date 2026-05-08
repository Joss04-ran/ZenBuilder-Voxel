using Unity.Mathematics;
using UnityEngine;


public static class OctaveNoise
{
    public static float Sample(float x, float z, NoiseSettings settings)
    {
        float value = 0f;
        float amplitude = 1f;
        float frequency = 1f;
        float maxValue = 0f;

        for (int i = 0; i < settings.Octaves; i++)
        {
            float sampleX = (x * frequency + settings.Seed) / settings.Scale;
            float sampleZ = (z * frequency + settings.Seed) / settings.Scale;

            value += Mathf.PerlinNoise(sampleX, sampleZ) * amplitude;
            maxValue += amplitude;

            amplitude *= settings.Persistence;
            frequency *= settings.Lacunarity;
        }
        return value / maxValue;
    }
}