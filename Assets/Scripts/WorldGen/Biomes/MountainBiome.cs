public class MountainBiome : Biome
{
    public override string Name => "Mountain";
    public override int MinHeight => 8;
    public override int MaxHeight => 28;

    public override NoiseSettings TerrainNoise => new NoiseSettings(
        scale: 50f, octaves: 5, persistence: 0.55f, lacunarity: 2.2f);
    public override int GetBlockID(int depth, int worldY)
    {
        if (worldY == 0) return BlockTypes.Bedrock.ID;
        if (worldY > 12) return BlockTypes.Stone.ID;
        if (depth == 0) return BlockTypes.Grass.ID;
        if (depth <= 2) return BlockTypes.Dirt.ID;
        return BlockTypes.Stone.ID;
    }
}