using UnityEngine;
using System.Collections.Generic;

public class StructureGenerator
{
    private Dictionary<int, List<StructurePlacementRule>> _biomeStructureRules;
    private TerrainGenerator _terrainGenerator;
    private const int MinSpacing = 6;

    public StructureGenerator(TerrainGenerator terrainGenerator)
    {
        _terrainGenerator = terrainGenerator;
        _biomeStructureRules = new Dictionary<int, List<StructurePlacementRule>>();
        RegisterDefaultRules();
    }
    private void RegisterDefaultRules()
    {
        // Biome 0 = Grassland
        RegisterStructure(0, new TreeStructure(4, 7), 0.05f);
        RegisterStructure(0, new CabinStructure(5, 5, 3), 0.002f);

        // Biome 1 = Desert — no trees, could add cactus here later
        // Biome 2 = Mountain — no structures for now
    }

    public void RegisterStructure(int biomeID, Structure structure, float spawnChance)
    {
        if (!_biomeStructureRules.ContainsKey(biomeID))
            _biomeStructureRules[biomeID] = new List<StructurePlacementRule>();

        _biomeStructureRules[biomeID].Add(new StructurePlacementRule(structure, spawnChance));
    }

    public void GenerateStructures(int[,,] voxelMap, Vector2Int chunkCoord,
        int chunkWidth, int chunkHeight)
    {
        HashSet<Vector2Int> usedPositions = new HashSet<Vector2Int>();

        for (int x = 0; x < chunkWidth; x++)
        {
            for (int z = 0; z < chunkWidth; z++)
            {
                int surfaceY = FindSurfaceY(voxelMap, x, z, chunkWidth, chunkHeight);
                if (surfaceY < 0) continue;

                float worldX = chunkCoord.x * chunkWidth + x;
                float worldZ = chunkCoord.y * chunkWidth + z;
                int biomeID = _terrainGenerator.GetBiomeIDAt(worldX, worldZ);

                if (!_biomeStructureRules.ContainsKey(biomeID)) continue;

                if (IsTooClose(usedPositions, x, z)) continue;

                TrySpawn(voxelMap, _biomeStructureRules[biomeID],
                    x, surfaceY, z, chunkWidth, chunkHeight, usedPositions);
            }
        }
    }

    private bool IsTooClose(HashSet<Vector2Int> used, int x, int z)
    {
        for (int dx = -MinSpacing; dx <= MinSpacing; dx++)
            for (int dz = -MinSpacing; dz <= MinSpacing; dz++)
                if (used.Contains(new Vector2Int(x + dx, z + dz)))
                    return true;
        return false;
    }

    private void TrySpawn(int[,,] voxelMap, List<StructurePlacementRule> rules,
        int x, int surfaceY, int z, int chunkWidth, int chunkHeight,
        HashSet<Vector2Int> usedPositions)
    {
        foreach (StructurePlacementRule rule in rules)
        {
            if (Random.value > rule.SpawnChance) continue;
            if (!rule.Structure.CanPlace(voxelMap, x, surfaceY, z, chunkWidth, chunkHeight)) continue;

            rule.Structure.Generate(voxelMap, x, surfaceY + 1, z, chunkWidth, chunkHeight);
            usedPositions.Add(new Vector2Int(x, z));
            break;
        }
    }

    private int FindSurfaceY(int[,,] voxelMap, int x, int z, int chunkWidth, int chunkHeight)
    {
        for (int y = chunkHeight - 1; y >= 0; y--)
            if (voxelMap[x, y, z] != BlockTypes.Air.ID)
                return y;
        return -1;
    }
}