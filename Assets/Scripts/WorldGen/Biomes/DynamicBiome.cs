using System.Collections.Generic;
public class DynamicBiome : Biome
{
    private BiomeEntry _config;
    private NoiseSettings _noiseSettings;

    public DynamicBiome(BiomeEntry config)
    {
        _config = config;
        _noiseSettings = new NoiseSettings(
            config.noise.scale,
            config.noise.octaves,
            config.noise.persistence,
            config.noise.lacunarity);
    }

    public override string Name => _config.name;
    public override int MinHeight => _config.minHeight;
    public override int MaxHeight => _config.maxHeight;
    public override NoiseSettings TerrainNoise => _noiseSettings;

    public int BiomeID => _config.id;
    public float TemperatureMin => _config.temperatureMin;
    public float TemperatureMax => _config.temperatureMax;
    public float HumidityMin => _config.humidityMin;
    public float HumidityMax => _config.humidityMax;
    public int WaterLevel => _config.waterLevel;
    public override int GetBlockID(int depth, int worldY)
    {
        if (worldY == 0) return BlockTypes.BedrockID;

        if (_config.surfaceStoneAboveY > 0 && worldY > _config.surfaceStoneAboveY)
            return BlockTypes.StoneID;

        foreach (LayerEntry layer in _config.layers)
            if (depth <= layer.maxDepth)
                return BlockTypes.GetIDByName(layer.blockName, BlockTypes.StoneID);

        return BlockTypes.StoneID;
    }
}