using System;
using System.Collections.Generic;
[Serializable]
public class OreEntry
{
    public string name;
    public string blockName;
    public int minY;
    public int maxY;
    public int veinSize;
    public float spawnChance;
    public int[] allowedBiomes;
}

[Serializable]
public class WorldSettings
{
    public int chunkWidth;
    public int chunkHeight;
    public int renderDistance;
    public int worldBorder;
    public float seed;
    public float biomeNoiseScale;
    public int biomeNoiseOctaves;
    public float continentalnessScale;
    public int continentalnessOctaves;
    public int blendRadius;
}

[Serializable]
public class CaveSettings
{
    public int wormCount;
    public int wormLength;
    public float wormRadius;
    public float stepSize;
    public float turnSpeed;
    public float directionNoiseScale;
    public int minCaveY;
    public int maxCaveY;
    public float seed;
}

[Serializable]
public class WorldGenConfigData
{
    public WorldSettings world;
    public CaveSettings cave;
    public List<OreEntry> ores;
}