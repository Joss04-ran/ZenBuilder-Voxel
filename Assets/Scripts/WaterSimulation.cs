using UnityEngine;

public class WaterSimulator
{
    public const byte SOURCE = 8;

    private static readonly int[] DX = { 1, -1, 0, 0 };
    private static readonly int[] DZ = { 0, 0, 1, -1 };
    private static byte[] _nextBuffer;

    public bool Tick(byte[,,] water, int[,,] solid, int w, int h)
    {
        int totalSize = w * h * w;
        if (!HasFlowingWater(water, totalSize, w)) return false;

        if (_nextBuffer == null || _nextBuffer.Length < totalSize)
            _nextBuffer = new byte[totalSize];

        System.Buffer.BlockCopy(water, 0, _nextBuffer, 0, totalSize);

        bool changed = false;

        for (int y = 1; y < h; y++)
            for (int x = 0; x < w; x++)
                for (int z = 0; z < w; z++)
                {
                    byte level = GetCell(water, x, y, z, w, h);
                    if (level == 0 || !IsPassable(solid, x, y, z)) continue;
                    if (!IsPassable(solid, x, y - 1, z)) continue;

                    byte below = GetCell(_nextBuffer, x, y - 1, z, w, h);
                    if (below >= SOURCE) continue;

                    if (below < level)
                    {
                        SetCell(_nextBuffer, x, y - 1, z, w, h, level);
                        if (level != SOURCE) SetCell(_nextBuffer, x, y, z, w, h, 0);
                        changed = true;
                    }
                }
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                for (int z = 0; z < w; z++)
                {
                    byte level = GetCell(_nextBuffer, x, y, z, w, h);
                    if (level < 2 || !IsPassable(solid, x, y, z)) continue;

                    bool blockedBelow = y == 0
                        || !IsPassable(solid, x, y - 1, z)
                        || GetCell(_nextBuffer, x, y - 1, z, w, h) >= SOURCE;

                    if (!blockedBelow && level != SOURCE) continue;

                    byte spread = level == SOURCE ? (byte)7 : (byte)(level - 1);

                    for (int i = 0; i < 4; i++)
                    {
                        int nx = x + DX[i], nz = z + DZ[i];
                        if (nx < 0 || nx >= w || nz < 0 || nz >= w) continue;
                        if (!IsPassable(solid, nx, y, nz)) continue;
                        if (GetCell(_nextBuffer, nx, y, nz, w, h) < spread)
                        {
                            SetCell(_nextBuffer, nx, y, nz, w, h, spread);
                            changed = true;
                        }
                    }
                }
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                for (int z = 0; z < w; z++)
                {
                    byte level = GetCell(_nextBuffer, x, y, z, w, h);
                    if (level == 0 || level == SOURCE || !IsPassable(solid, x, y, z)) continue;

                    bool replenished = false;

                    if (y < h - 1 && GetCell(_nextBuffer, x, y + 1, z, w, h) > 0)
                        replenished = true;

                    for (int i = 0; i < 4 && !replenished; i++)
                    {
                        int nx = x + DX[i], nz = z + DZ[i];
                        if (nx < 0 || nx >= w || nz < 0 || nz >= w) continue;
                        if (GetCell(_nextBuffer, nx, y, nz, w, h) >= level) replenished = true;
                    }

                    if (!replenished)
                    {
                        SetCell(_nextBuffer, x, y, z, w, h, 0);
                        changed = true;
                    }
                }
        if (changed)
            System.Buffer.BlockCopy(_nextBuffer, 0, water, 0, totalSize);

        return changed;
    }
    private bool HasFlowingWater(byte[,,] water, int w, int h)
    {
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                for (int z = 0; z < w; z++)
                {
                    byte b = water[x, y, z];
                    if (b > 0 && b < SOURCE) return true;
                }
            }
        }
        return false;
    }
    private byte GetCell(byte[] buf, int x, int y, int z, int w, int h)
        => buf[x + w * (y + h * z)];
    private void SetCell(byte[] buf, int x, int y, int z, int w, int h, byte val)
        => buf[x + w * (y + h * z)] = val;
    private byte GetCell(byte[,,] arr, int x, int y, int z, int w, int h)
        => arr[x, y, z];

    private bool IsPassable(int[,,] solid, int x, int y, int z)
    {
        if (x < 0 || x >= solid.GetLength(0)) return false;
        if (y < 0 || y >= solid.GetLength(1)) return false;
        if (z < 0 || z >= solid.GetLength(2)) return false;
        return solid[x, y, z] == BlockTypes.AirID;
    }
}