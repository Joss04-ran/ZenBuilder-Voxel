using UnityEngine;

public class WormCaveGenerator
{
    private CaveSettings _settings;

    public WormCaveGenerator(CaveSettings settings = null)
    {
        _settings = settings ?? new CaveSettings();
    }
    public void GenerateCaves(int[,,] voxelMap, Vector2Int chunkCoord,
        int chunkWidth, int chunkHeight)
    {
        for (int i = 0; i < _settings.WormCount; i++)
            SpawnWorm(voxelMap, chunkCoord, chunkWidth, chunkHeight, i);
    }

    private void SpawnWorm(int[,,] voxelMap, Vector2Int chunkCoord,
        int chunkWidth, int chunkHeight, int wormIndex)
    {
        float seedX = chunkCoord.x * 31.7f + wormIndex * 47.3f + _settings.Seed;
        float seedZ = chunkCoord.y * 29.1f + wormIndex * 53.9f + _settings.Seed;

        float startX = chunkCoord.x * chunkWidth
            + Mathf.PerlinNoise(seedX, 0f) * chunkWidth;
        float startY = _settings.MinCaveY
            + Mathf.PerlinNoise(0f, seedZ) * (_settings.MaxCaveY - _settings.MinCaveY);
        float startZ = chunkCoord.y * chunkWidth
            + Mathf.PerlinNoise(seedX, seedZ) * chunkWidth;

        float yaw = Mathf.PerlinNoise(seedX * 2f, seedZ) * 360f;
        float pitch = (Mathf.PerlinNoise(seedX, seedZ * 2f) - 0.5f) * 60f;

        float noiseOffsetA = wormIndex * 137.5f + _settings.Seed;
        float noiseOffsetB = wormIndex * 256.3f + _settings.Seed;

        Vector3 pos = new Vector3(startX, startY, startZ);

        for (int step = 0; step < _settings.WormLength; step++)
        {
            int lx = Mathf.FloorToInt(pos.x) - chunkCoord.x * chunkWidth;
            int ly = Mathf.FloorToInt(pos.y);
            int lz = Mathf.FloorToInt(pos.z) - chunkCoord.y * chunkWidth;

            CarveSphere(voxelMap, lx, ly, lz, _settings.WormRadius, chunkWidth, chunkHeight);
            float t = (float)step / _settings.WormLength;

            float yawNoise = Mathf.PerlinNoise(t * _settings.DirectionNoiseScale + noiseOffsetA, 0f);
            float pitchNoise = Mathf.PerlinNoise(0f, t * _settings.DirectionNoiseScale + noiseOffsetB);

            yaw += (yawNoise - 0.5f) * 2f * _settings.TurnSpeed;
            pitch += (pitchNoise - 0.5f) * 2f * _settings.TurnSpeed * 0.4f;

            pitch = Mathf.Clamp(pitch, -40f, 40f);

            float radYaw = yaw * Mathf.Deg2Rad;
            float radPitch = pitch * Mathf.Deg2Rad;

            Vector3 dir = new Vector3(
                Mathf.Cos(radPitch) * Mathf.Sin(radYaw),
                Mathf.Sin(radPitch),
                Mathf.Cos(radPitch) * Mathf.Cos(radYaw)
            );

            pos += dir * _settings.StepSize;

            if (pos.y < _settings.MinCaveY || pos.y > _settings.MaxCaveY) break;
        }
    }
    private void CarveSphere(int[,,] voxelMap, int cx, int cy, int cz,
        float radius, int chunkWidth, int chunkHeight)
    {
        int r = Mathf.CeilToInt(radius);

        for (int dx = -r; dx <= r; dx++)
        {
            for (int dy = -r; dy <= r; dy++)
            {
                for (int dz = -r; dz <= r; dz++)
                {
                    if (dx * dx + dy * dy + dz * dz > radius * radius) continue;

                    int x = cx + dx;
                    int y = cy + dy;
                    int z = cz + dz;

                    if (x < 0 || x >= chunkWidth) continue;
                    if (y <= 0 || y >= chunkHeight) continue;
                    if (z < 0 || z >= chunkWidth) continue;

                    voxelMap[x, y, z] = BlockTypes.Air.ID;
                }
            }
        }
    }
}