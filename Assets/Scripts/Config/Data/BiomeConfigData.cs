using System;
using System.Collections.Generic;
[Serializable]
public class BiomeNoiseConfig
{
    public float scale;
    public int octaves;
    public float persistence;
    public float lacunarity;
}
[Serializable]
public class LayerEntry
{
    public string blockName;
    public int maxDepth;
}
[Serializable]
public class BiomeEntry
{
    public int id;
    public string name;

    public float temperatureMin;
    public float temperatureMax;
    public float humidityMin;
    public float humidityMax;

    public int minHeight;
    public int maxHeight;
    public int surfaceStoneAboveY;
    public int waterLevel;

    public BiomeNoiseConfig noise;
    public List<LayerEntry> layers;
}
[Serializable]
public class BiomeConfigData
{
    public List<BiomeEntry> biomes;
}