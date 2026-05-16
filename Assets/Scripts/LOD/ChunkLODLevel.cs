using UnityEngine;

public enum LODLevel
{
    Full = 0, // All geometry, including caves
    Medium = 1, // Surface + caves above CaveCutoffY
    Far = 2  // Sky-exposed voxels only (heightmap-style)
}

public static class ChunkLODSettings
{
    public const int CaveCutoffY = 8;
    public static LODLevel GetLevel(int chunkDist)
    {
        if (chunkDist <= 2) return LODLevel.Full;
        if (chunkDist <= 5) return LODLevel.Medium;
        return LODLevel.Far;
    }

    public static int ChunkDistance(Vector2Int a, Vector2Int b)
        => Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
}