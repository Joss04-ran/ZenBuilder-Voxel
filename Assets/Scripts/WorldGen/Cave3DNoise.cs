using UnityEngine;
public static class Cave3DNoise
{
    public static float Sample(float x, float y, float z, float scale, float seed = 0f)
    {
        float x1 = x * scale + seed;
        float y1 = y * scale + seed;
        float z1 = z * scale + seed;

        float xy = Mathf.PerlinNoise(x1, y1);
        float xz = Mathf.PerlinNoise(x1, z1);
        float yz = Mathf.PerlinNoise(y1, z1);
        float yx = Mathf.PerlinNoise(y1, x1);
        float zx = Mathf.PerlinNoise(z1, x1);
        float zy = Mathf.PerlinNoise(z1, y1);

        return (xy + xz + yz + yx + zx + zy) / 6f;
    }
}