using System;
using System.Collections.Generic;
[Serializable]
public class OreData
{
    public string name;
    public int blockID;
    public int minY;
    public int maxY;
    public int veinSize;
    public float spawnChance;
    public int[] allowedBiomes;
}
[Serializable]
public class OreDatabase
{
    public List<OreData> ores;
}