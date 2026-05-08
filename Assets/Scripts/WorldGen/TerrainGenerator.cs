using UnityEngine;
using System.Collections.Generic;

public class TerrainGenerator
{
    private List<Biome> _biomes;
    private NoiseSettings _temperatureNoise;
    private NoiseSettings _humidityNoise;

    public TerrainGenerator(float worldSeed = 0f)
    {
        _temperatureNoise = new NoiseSettings(200f, 2, 0.5f, 2f, worldSeed);
        _humidityNoise = new NoiseSettings(200f, 2, 0.5f, 2f, worldSeed + 1000f);

        _biomes = new List<Biome>
        {
            new GrasslandBiome(),  
            new DesertBiome(),     
            new MountainBiome()    
        };
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
                int surfaceY = GetSurfaceHeight(worldX, worldZ, biome, chunkHeight);
                FillColumn(voxelMap, x, z, surfaceY, chunkHeight, biome);
            }
        }
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
    {
        return _biomes[GetBiomeIDAt(worldX, worldZ)];
    }

    private int GetSurfaceHeight(float worldX, float worldZ, Biome biome, int chunkHeight)
    {
        float noise = OctaveNoise.Sample(worldX, worldZ, biome.TerrainNoise);
        int height = Mathf.FloorToInt(Mathf.Lerp(biome.MinHeight, biome.MaxHeight, noise));
        return Mathf.Clamp(height, 0, chunkHeight - 1);
    }

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