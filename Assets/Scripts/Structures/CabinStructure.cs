using UnityEngine;
public class CabinStructure : Structure
{
    private int _width;
    private int _depth;
    private int _wallHeight;

    public CabinStructure(int width = 5, int depth = 5, int wallHeight = 3)
    {
        _width = width;
        _depth = depth;
        _wallHeight = wallHeight;
    }

    public override void Generate(int[,,] voxelMap, int originX, int originY, int originZ,
        int chunkWidth, int chunkHeight)
    {
        int groundY = FindLowestGround(voxelMap, originX, originY, originZ, chunkWidth, chunkHeight);

        BackfillTerrain(voxelMap, originX, groundY, originZ, chunkWidth, chunkHeight);
        PlaceFloor(voxelMap, originX, groundY, originZ, chunkWidth, chunkHeight);
        PlaceWalls(voxelMap, originX, groundY, originZ, chunkWidth, chunkHeight);
        PlaceRoof(voxelMap, originX, groundY, originZ, chunkWidth, chunkHeight);
    }
    private int FindLowestGround(int[,,] voxelMap, int ox, int oy, int oz,
        int chunkWidth, int chunkHeight)
    {
        int minY = oy;

        for (int x = 0; x < _width; x++)
        {
            for (int z = 0; z < _depth; z++)
            {
                int wx = ox + x;
                int wz = oz + z;

                if (wx < 0 || wx >= chunkWidth || wz < 0 || wz >= chunkWidth) continue;
                for (int y = oy; y >= 0; y--)
                {
                    if (voxelMap[wx, y, wz] != BlockTypes.Air.ID)
                    {
                        if (y < minY) minY = y;
                        break;
                    }
                }
            }
        }

        return minY;
    }
    private void BackfillTerrain(int[,,] voxelMap, int ox, int groundY, int oz,
        int chunkWidth, int chunkHeight)
    {
        for (int x = 0; x < _width; x++)
        {
            for (int z = 0; z < _depth; z++)
            {
                for (int y = groundY; y <= groundY + _wallHeight + 2; y++)
                {
                    int wx = ox + x;
                    int wz = oz + z;
                    if (wx < 0 || wx >= chunkWidth || wz < 0 || wz >= chunkWidth) continue;
                    if (voxelMap[wx, y, wz] == BlockTypes.Air.ID && y < groundY)
                        TryPlaceBlock(voxelMap, wx, y, wz,
                            BlockTypes.Dirt.ID, chunkWidth, chunkHeight, true);
                }
            }
        }
    }

    private void PlaceFloor(int[,,] voxelMap, int ox, int groundY, int oz,
        int chunkWidth, int chunkHeight)
    {
        for (int x = 0; x < _width; x++)
            for (int z = 0; z < _depth; z++)
                TryPlaceBlock(voxelMap, ox + x, groundY, oz + z,
                    BlockTypes.Cobblestone.ID, chunkWidth, chunkHeight, true);
    }

    private void PlaceWalls(int[,,] voxelMap, int ox, int groundY, int oz,
        int chunkWidth, int chunkHeight)
    {
        for (int y = 1; y <= _wallHeight; y++)
            for (int x = 0; x < _width; x++)
                for (int z = 0; z < _depth; z++)
                {
                    bool isPerimeter = x == 0 || x == _width - 1 || z == 0 || z == _depth - 1;
                    if (!isPerimeter) continue;
                    TryPlaceBlock(voxelMap, ox + x, groundY + y, oz + z,
                        BlockTypes.Brick.ID, chunkWidth, chunkHeight, true);
                }
    }

    private void PlaceRoof(int[,,] voxelMap, int ox, int groundY, int oz,
        int chunkWidth, int chunkHeight)
    {
        int roofY = groundY + _wallHeight + 1;
        for (int x = -1; x <= _width; x++)
            for (int z = -1; z <= _depth; z++)
                TryPlaceBlock(voxelMap, ox + x, roofY, oz + z,
                    BlockTypes.Plank.ID, chunkWidth, chunkHeight, true);
    }

    public override bool CanPlace(int[,,] voxelMap, int x, int surfaceY, int z,
        int chunkWidth, int chunkHeight)
    {
        int buffer = Mathf.Max(_width, _depth) / 2 + 1;
        return x >= buffer && x < chunkWidth - buffer - _width &&
               z >= buffer && z < chunkWidth - buffer - _depth;
    }
}