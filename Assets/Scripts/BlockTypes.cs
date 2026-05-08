using UnityEngine;

public static class BlockTypes
{
    public readonly struct BlockData
    {
        public readonly int ID;
        public readonly string Name;

        public BlockData(int id, string name)
        {
            ID = id;
            Name = name;
        }
    }

    public static readonly BlockData Air = new BlockData(0, "Air");
    public static readonly BlockData Bedrock = new BlockData(69, "Bedrock");
    public static readonly BlockData Stone = new BlockData(40, "Stone");
    public static readonly BlockData Dirt = new BlockData(53, "Dirt");
    public static readonly BlockData Log = new BlockData(2, "Log");
    public static readonly BlockData Leaves = new BlockData(77, "Leaves");
    public static readonly BlockData Grass = new BlockData(18, "Grass");
    public static readonly BlockData Plank = new BlockData(10, "Plank");
    public static readonly BlockData Cobblestone = new BlockData(87, "Cobblestone");
    public static readonly BlockData Brick = new BlockData(1, "Brick");
    public static readonly BlockData Glass = new BlockData(88, "Glass");
    public static readonly BlockData Sand = new BlockData(58, "Sand");
    public static readonly BlockData Sandstone = new BlockData(62, "Sandstone");
}