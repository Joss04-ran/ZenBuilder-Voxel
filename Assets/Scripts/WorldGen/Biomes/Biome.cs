using UnityEngine;
public abstract class Biome
{
    public abstract string Name { get; }
    public abstract int MinHeight { get; }
    public abstract int MaxHeight { get; }

    public abstract NoiseSettings TerrainNoise { get; }
    public abstract int GetBlockID(int depth, int worldY);
}