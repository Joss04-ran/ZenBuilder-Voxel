using UnityEngine;
using System.Collections.Generic;

public class TerrainGenerator
{
    private List<Biome> _biomes;
    private NoiseSettings _temperatureNoise;
    private NoiseSettings _humidityNoise;
    private NoiseSettings _continentalnessNoise;
    private WormCaveGenerator _caveGenerator;

    private const int BlendRadius = 8;

    public TerrainGenerator(float worldSeed = 0f, CaveSettings caveSettings = null)
    {
        _temperatureNoise = new NoiseSettings(200f, 2, 0.5f, 2f, worldSeed);
        _humidityNoise = new NoiseSettings(200f, 2, 0.5f, 2f, worldSeed + 1000f);
        _continentalnessNoise = new NoiseSettings(400f, 3, 0.5f, 2f, worldSeed + 2000f);

        _biomes = new List<Biome>
        {
            new GrasslandBiome(),
            new DesertBiome(),
            new MountainBiome()
        };

        _caveGenerator = new WormCaveGenerator(caveSettings);
    }

    public void GenerateTerrain(int[,,] voxelMap, Vector2Int chunkCoord,
        int chunkWidth, int chunkHeight)
    {
        for (int x = 0; x < chunkWidth; x++)
        {
            for (int z = 0; z < chunkWidth; z++)
            {
                float worldX = chunkCoord.x * chunkWidth + x;
                float worldZ = chunkCoord.y * chunkWidth + z;

                Biome biome = GetBiomeAt(worldX, worldZ);
                int surfaceY = GetBlendedSurfaceHeight(worldX, worldZ, chunkHeight);

                FillColumn(voxelMap, x, z, surfaceY, chunkHeight, biome);
            }
        }
        _caveGenerator.GenerateCaves(voxelMap, chunkCoord, chunkWidth, chunkHeight);
    }
    private int GetBlendedSurfaceHeight(float worldX, float worldZ, int chunkHeight)
    {
        Vector2[] offsets = {
            Vector2.zero,
            new Vector2( BlendRadius, 0),
            new Vector2(-BlendRadius, 0),
            new Vector2(0,  BlendRadius),
            new Vector2(0, -BlendRadius)
        };
        float[] weights = { 4f, 1f, 1f, 1f, 1f };

        float totalHeight = 0f;
        float totalWeight = 0f;

        for (int i = 0; i < offsets.Length; i++)
        {
            float sx = worldX + offsets[i].x;
            float sz = worldZ + offsets[i].y;
            Biome biome = GetBiomeAt(sx, sz);

            float noise = OctaveNoise.Sample(sx, sz, biome.TerrainNoise);
            float height = Mathf.Lerp(biome.MinHeight, biome.MaxHeight, noise);

            totalHeight += height * weights[i];
            totalWeight += weights[i];
        }
        float continentalness = OctaveNoise.Sample(worldX, worldZ, _continentalnessNoise);
        float continentalShift = Mathf.Lerp(-2f, 4f, continentalness);

        int finalHeight = Mathf.RoundToInt(totalHeight / totalWeight + continentalShift);
        return Mathf.Clamp(finalHeight, 1, chunkHeight - 1);
    }

    public int GetBiomeIDAt(float worldX, float worldZ)
    {
        float temperature = OctaveNoise.Sample(worldX, worldZ, _temperatureNoise);
        float humidity = OctaveNoise.Sample(worldX, worldZ, _humidityNoise);

        if (temperature > 0.6f && humidity < 0.4f) return 1;
        if (temperature < 0.4f && humidity > 0.5f) return 2;
        return 0;
    }

    private Biome GetBiomeAt(float worldX, float worldZ)
        => _biomes[GetBiomeIDAt(worldX, worldZ)];

    private void FillColumn(int[,,] voxelMap, int x, int z,
        int surfaceY, int chunkHeight, Biome biome)
    {
        for (int y = 0; y < chunkHeight; y++)
        {
            if (y > surfaceY) { voxelMap[x, y, z] = BlockTypes.Air.ID; continue; }
            voxelMap[x, y, z] = biome.GetBlockID(surfaceY - y, y);
        }
    }
}