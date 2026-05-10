using UnityEngine;
using System.Collections.Generic;

public class TerrainGenerator
{
    private List<DynamicBiome> _biomes;
    private NoiseSettings _temperatureNoise;
    private NoiseSettings _humidityNoise;
    private NoiseSettings _continentalnessNoise;
    private WormCaveGenerator _caveGenerator;
    private OreGenerator _oreGenerator;
    private int _blendRadius;
    public TerrainGenerator()
    {
        WorldSettings world = ConfigManager.Instance?.WorldConfig?.world ?? new WorldSettings
        {
            seed = 0,
            biomeNoiseScale = 200,
            biomeNoiseOctaves = 2,
            continentalnessScale = 400,
            continentalnessOctaves = 3,
            blendRadius = 8
        };

        _blendRadius = world.blendRadius;
        _temperatureNoise = new NoiseSettings(world.biomeNoiseScale, world.biomeNoiseOctaves, 0.5f, 2f, world.seed);
        _humidityNoise = new NoiseSettings(world.biomeNoiseScale, world.biomeNoiseOctaves, 0.5f, 2f, world.seed + 1000f);
        _continentalnessNoise = new NoiseSettings(world.continentalnessScale, world.continentalnessOctaves, 0.5f, 2f, world.seed + 2000f);

        _biomes = new List<DynamicBiome>();
        BiomeConfigData biomeConfig = ConfigManager.Instance?.BiomeConfig;
        if (biomeConfig?.biomes != null)
            foreach (BiomeEntry entry in biomeConfig.biomes)
                _biomes.Add(new DynamicBiome(entry));

        CaveSettings caveConfig = ConfigManager.Instance?.WorldConfig?.cave ?? new CaveSettings
        {
            wormCount = 3,
            wormLength = 80,
            wormRadius = 2.2f,
            stepSize = 1.5f,
            turnSpeed = 35f,
            directionNoiseScale = 3f,
            minCaveY = 1,
            maxCaveY = 18,
            seed = 500f
        };

        _caveGenerator = new WormCaveGenerator(caveConfig);
        _oreGenerator = new OreGenerator();
    }

    public void GenerateTerrain(int[,,] voxelMap, Vector2Int chunkCoord,
        int chunkWidth, int chunkHeight)
    {
        for (int x = 0; x < chunkWidth; x++)
            for (int z = 0; z < chunkWidth; z++)
            {
                float worldX = chunkCoord.x * chunkWidth + x;
                float worldZ = chunkCoord.y * chunkWidth + z;
                DynamicBiome biome = GetBiomeAt(worldX, worldZ);
                int surfaceY = GetBlendedHeight(worldX, worldZ, chunkHeight);
                FillColumn(voxelMap, x, z, surfaceY, chunkHeight, biome);
            }

        _oreGenerator.GenerateOres(voxelMap, chunkCoord, chunkWidth, chunkHeight, this);
        _caveGenerator.GenerateCaves(voxelMap, chunkCoord, chunkWidth, chunkHeight);
    }
    public int GetBiomeIDAt(float worldX, float worldZ)
    {
        float temp = OctaveNoise.Sample(worldX, worldZ, _temperatureNoise);
        float humidity = OctaveNoise.Sample(worldX, worldZ, _humidityNoise);

        foreach (DynamicBiome biome in _biomes)
        {
            if (temp >= biome.TemperatureMin && temp <= biome.TemperatureMax &&
                humidity >= biome.HumidityMin && humidity <= biome.HumidityMax)
                return biome.BiomeID;
        }
        return 0;
    }

    private DynamicBiome GetBiomeAt(float worldX, float worldZ)
    {
        int id = GetBiomeIDAt(worldX, worldZ);
        foreach (DynamicBiome b in _biomes)
            if (b.BiomeID == id) return b;
        return _biomes.Count > 0 ? _biomes[0] : null;
    }

    private int GetBlendedHeight(float worldX, float worldZ, int chunkHeight)
    {
        Vector2[] offsets = {
            Vector2.zero,
            new Vector2( _blendRadius, 0), new Vector2(-_blendRadius, 0),
            new Vector2(0,  _blendRadius), new Vector2(0, -_blendRadius)
        };
        float[] weights = { 4f, 1f, 1f, 1f, 1f };

        float total = 0f, totalW = 0f;
        for (int i = 0; i < offsets.Length; i++)
        {
            DynamicBiome b = GetBiomeAt(worldX + offsets[i].x, worldZ + offsets[i].y);
            if (b == null) continue;
            float noise = OctaveNoise.Sample(worldX + offsets[i].x, worldZ + offsets[i].y, b.TerrainNoise);
            total += Mathf.Lerp(b.MinHeight, b.MaxHeight, noise) * weights[i];
            totalW += weights[i];
        }

        float continental = OctaveNoise.Sample(worldX, worldZ, _continentalnessNoise);
        return Mathf.Clamp(Mathf.RoundToInt(total / totalW + Mathf.Lerp(-2f, 4f, continental)), 1, chunkHeight - 1);
    }

    private void FillColumn(int[,,] voxelMap, int x, int z,
        int surfaceY, int chunkHeight, DynamicBiome biome)
    {
        int waterLevel = biome?.WaterLevel ?? 0;

        for (int y = 0; y < chunkHeight; y++)
        {
            if (y <= surfaceY)
            {
                voxelMap[x, y, z] = biome != null
                    ? biome.GetBlockID(surfaceY - y, y)
                    : BlockTypes.StoneID;
            }
            else if (waterLevel > 0 && y <= waterLevel)
            {
                voxelMap[x, y, z] = GetWaterBlock(waterLevel - y);
            }
            else
            {
                voxelMap[x, y, z] = BlockTypes.AirID;
            }
        }
    }
    private int GetWaterBlock(int depthFromSurface)
    {
        if (depthFromSurface == 0) return BlockTypes.WaterID;
        if (depthFromSurface <= 2) return BlockTypes.WaterMidID;
        return BlockTypes.WaterDeepID;
    }
}