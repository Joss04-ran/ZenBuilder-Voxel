public class GrasslandBiome : Biome
{
    public override string Name => "Grassland";
    public override int MinHeight => 4;
    public override int MaxHeight => 14;

    public override NoiseSettings TerrainNoise => new NoiseSettings(
        scale: 80f, octaves: 4, persistence: 0.5f, lacunarity: 2f);
    public override int GetBlockID(int depth, int worldY)
    {
        if (worldY == 0) return BlockTypes.Bedrock.ID;
        if (depth == 0) return BlockTypes.Grass.ID;
        if (depth <= 3) return BlockTypes.Dirt.ID;
        return BlockTypes.Stone.ID;
    }
}