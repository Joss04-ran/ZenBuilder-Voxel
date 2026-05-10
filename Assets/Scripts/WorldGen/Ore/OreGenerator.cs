using UnityEngine;
using System.Collections.Generic;

public class OreGenerator
{
    private List<OreEntry> _oreTypes;
    public OreGenerator()
    {
        _oreTypes = ConfigManager.Instance?.WorldConfig?.ores ?? new List<OreEntry>();

        if (_oreTypes.Count == 0)
            Debug.LogWarning("[OreGenerator] No ore entries found in WorldGenConfig");
        else
            Debug.Log($"[OreGenerator] Loaded {_oreTypes.Count} ore types");
    }

    public void GenerateOres(int[,,] voxelMap, Vector2Int chunkCoord,
        int chunkWidth, int chunkHeight, TerrainGenerator terrainGenerator)
    {
        foreach (OreEntry ore in _oreTypes)
            PlaceOreType(voxelMap, chunkCoord, chunkWidth, chunkHeight, ore, terrainGenerator);
    }

    private void PlaceOreType(int[,,] voxelMap, Vector2Int chunkCoord,
        int chunkWidth, int chunkHeight, OreEntry ore, TerrainGenerator terrainGenerator)
    {
        int oreBlockID = BlockTypes.GetIDByName(ore.blockName, 0);
        if (oreBlockID == 0) { Debug.LogWarning($"[OreGenerator] Unknown block: {ore.blockName}"); return; }

        for (int x = 0; x < chunkWidth; x++)
        {
            for (int z = 0; z < chunkWidth; z++)
            {
                float worldX = chunkCoord.x * chunkWidth + x;
                float worldZ = chunkCoord.y * chunkWidth + z;

                int biomeID = terrainGenerator.GetBiomeIDAt(worldX, worldZ);
                if (!IsBiomeAllowed(ore, biomeID)) continue;
                if (Random.value > ore.spawnChance) continue;

                int oreY = Random.Range(ore.minY, Mathf.Min(ore.maxY, chunkHeight - 2));
                if (voxelMap[x, oreY, z] != BlockTypes.StoneID) continue;

                PlaceVein(voxelMap, x, oreY, z, ore.veinSize, oreBlockID, chunkWidth, chunkHeight);
            }
        }
    }

    private void PlaceVein(int[,,] voxelMap, int ox, int oy, int oz,
        int veinSize, int blockID, int chunkWidth, int chunkHeight)
    {
        int radius = Mathf.CeilToInt(Mathf.Pow(veinSize, 0.4f));

        for (int dx = -radius; dx <= radius; dx++)
            for (int dy = -radius; dy <= radius; dy++)
                for (int dz = -radius; dz <= radius; dz++)
                {
                    int x = ox + dx, y = oy + dy, z = oz + dz;
                    if (x < 0 || x >= chunkWidth || y <= 0 || y >= chunkHeight || z < 0 || z >= chunkWidth) continue;

                    float dist = Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
                    if (Random.value > 1f - dist / (radius + 1f)) continue;
                    if (voxelMap[x, y, z] != BlockTypes.StoneID) continue;

                    voxelMap[x, y, z] = blockID;
                }
    }

    private bool IsBiomeAllowed(OreEntry ore, int biomeID)
    {
        foreach (int allowed in ore.allowedBiomes)
            if (allowed == biomeID) return true;
        return false;
    }
}