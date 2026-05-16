using UnityEngine;
using System.Collections.Generic;
public static class ChunkVisibilityComputer
{
    private static readonly int[] DX = { 0, 0, 0, 0, -1, 1 };
    private static readonly int[] DY = { 0, 0, 1, -1, 0, 0 };
    private static readonly int[] DZ = { -1, 1, 0, 0, 0, 0 };
    public static ChunkFaceVisibility Compute(int[,,] voxelMap, int width, int height)
    {
        ChunkFaceVisibility result = new ChunkFaceVisibility();
        bool[,,] visited = new bool[width, height, width];

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                for (int z = 0; z < width; z++)
                {
                    if (visited[x, y, z]) continue;
                    if (voxelMap[x, y, z] != BlockTypes.AirID) continue;

                    bool[] touchedFaces = FloodFill(voxelMap, visited, x, y, z, width, height);

                    for (int f1 = 0; f1 < ChunkFaceVisibility.FaceCount; f1++)
                    {
                        if (!touchedFaces[f1]) continue;
                        for (int f2 = f1 + 1; f2 < ChunkFaceVisibility.FaceCount; f2++)
                            if (touchedFaces[f2])
                                result.Connect(f1, f2);
                    }
                }

        return result;
    }

    private static bool[] FloodFill(int[,,] map, bool[,,] visited,
        int sx, int sy, int sz, int w, int h)
    {
        bool[] faces = new bool[ChunkFaceVisibility.FaceCount];
        var queue = new Queue<Vector3Int>();

        queue.Enqueue(new Vector3Int(sx, sy, sz));
        visited[sx, sy, sz] = true;

        while (queue.Count > 0)
        {
            Vector3Int c = queue.Dequeue();

            // Record which chunk-face boundary this voxel touches
            if (c.x == 0) faces[4] = true; // Left   (-X)
            if (c.x == w - 1) faces[5] = true; // Right  (+X)
            if (c.y == 0) faces[3] = true; // Bottom (-Y)
            if (c.y == h - 1) faces[2] = true; // Top    (+Y)
            if (c.z == 0) faces[0] = true; // Back   (-Z)
            if (c.z == w - 1) faces[1] = true; // Front  (+Z)

            for (int d = 0; d < 6; d++)
            {
                int nx = c.x + DX[d];
                int ny = c.y + DY[d];
                int nz = c.z + DZ[d];

                if (nx < 0 || nx >= w || ny < 0 || ny >= h || nz < 0 || nz >= w) continue;
                if (visited[nx, ny, nz]) continue;
                if (map[nx, ny, nz] != BlockTypes.AirID) continue;

                visited[nx, ny, nz] = true;
                queue.Enqueue(new Vector3Int(nx, ny, nz));
            }
        }

        return faces;
    }
}