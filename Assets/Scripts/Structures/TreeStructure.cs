using UnityEngine;

public class TreeStructure : Structure
{
    private int _logBlockID;
    private int _leafBlockID;
    private int _minTrunkHeight;
    private int _maxTrunkHeight;

    public TreeStructure(int minTrunk = 4, int maxTrunk = 7)
    {
        _logBlockID = BlockTypes.Log.ID;
        _leafBlockID = BlockTypes.Leaves.ID;
        _minTrunkHeight = minTrunk;
        _maxTrunkHeight = maxTrunk;
    }

    public override void Generate(int[,,] voxelMap, int originX, int originY, int originZ,
        int chunkWidth, int chunkHeight)
    {
        int trunkHeight = Random.Range(_minTrunkHeight, _maxTrunkHeight);
        PlaceTrunk(voxelMap, originX, originY, originZ, trunkHeight, chunkWidth, chunkHeight);
        PlaceLeaves(voxelMap, originX, originY, originZ, trunkHeight, chunkWidth, chunkHeight);
    }

    private void PlaceTrunk(int[,,] voxelMap, int x, int baseY, int z,
        int trunkHeight, int chunkWidth, int chunkHeight)
    {
        for (int y = 0; y < trunkHeight; y++)
            TryPlaceBlock(voxelMap, x, baseY + y, z, _logBlockID, chunkWidth, chunkHeight, true);
    }

    private void PlaceLeaves(int[,,] voxelMap, int x, int baseY, int z,
        int trunkHeight, int chunkWidth, int chunkHeight)
    {
        int leafTop = baseY + trunkHeight;

        for (int ly = leafTop - 2; ly <= leafTop; ly++)
        {
            int radius = (ly == leafTop) ? 1 : 2;

            for (int lx = -radius; lx <= radius; lx++)
            {
                for (int lz = -radius; lz <= radius; lz++)
                {
                    if (Mathf.Abs(lx) == radius && Mathf.Abs(lz) == radius) continue;
                    TryPlaceBlock(voxelMap, x + lx, ly, z + lz,
                        _leafBlockID, chunkWidth, chunkHeight);
                }
            }
        }
    }

    public override bool CanPlace(int[,,] voxelMap, int x, int surfaceY, int z,
        int chunkWidth, int chunkHeight)
    {
        return x >= 2 && x < chunkWidth - 2 && z >= 2 && z < chunkWidth - 2;
    }
}