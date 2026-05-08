public class DesertBiome : Biome
{
    public override string Name => "Desert";
    public override int MinHeight => 3;
    public override int MaxHeight => 8;

    public override NoiseSettings TerrainNoise => new NoiseSettings(
        scale: 120f, octaves: 3, persistence: 0.4f, lacunarity: 1.8f);

    public override int GetBlockID(int depth, int worldY)
    {
        if (worldY == 0) return BlockTypes.Bedrock.ID;
        if (depth <= 1) return BlockTypes.Sand.ID;
        if (depth <= 3) return BlockTypes.Sandstone.ID;
        return BlockTypes.Stone.ID;
    }
}