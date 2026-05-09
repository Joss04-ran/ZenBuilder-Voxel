public class GrasslandBiome : Biome
{
    public override string Name => "Grassland";
    public override int MinHeight => 6;
    public override int MaxHeight => 18;

    public override NoiseSettings TerrainNoise => new NoiseSettings(
        scale: 70f, octaves: 5, persistence: 0.55f, lacunarity: 2f);

    public override int GetBlockID(int depth, int worldY)
    {
        if (worldY == 0) return BlockTypes.Bedrock.ID;
        if (depth == 0) return BlockTypes.Grass.ID;
        if (depth <= 3) return BlockTypes.Dirt.ID;
        return BlockTypes.Stone.ID;
    }
}