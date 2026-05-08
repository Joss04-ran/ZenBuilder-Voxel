using UnityEngine;
public abstract class Structure
{
    public abstract void Generate(int[,,] voxelMap, int originX, int originY, int originZ,
        int chunkWidth, int chunkHeight);
    public virtual bool CanPlace(int[,,] voxelMap, int x, int surfaceY, int z,
        int chunkWidth, int chunkHeight)
    {
        return true;
    }
    protected bool IsInBounds(int x, int y, int z, int chunkWidth, int chunkHeight)
    {
        return x >= 0 && x < chunkWidth
            && y >= 0 && y < chunkHeight
            && z >= 0 && z < chunkWidth;
    }
    protected void TryPlaceBlock(int[,,] voxelMap, int x, int y, int z,
        int blockID, int chunkWidth, int chunkHeight, bool overwrite = false)
    {
        if (!IsInBounds(x, y, z, chunkWidth, chunkHeight)) return;
        if (!overwrite && voxelMap[x, y, z] != 0) return;
        voxelMap[x, y, z] = blockID;
    }
}