using UnityEngine;
using System.Collections.Generic;

public static class BlockTypes
{
    public static HashSet<int> TransparentBlocks { get; private set; } = new HashSet<int>();
    public static bool IsTransparent(int id) => TransparentBlocks.Contains(id);

    public readonly struct BlockData
    {
        public readonly int ID;
        public readonly string Name;
        public BlockData(int id, string name) { ID = id; Name = name; }
    }
    public static int AirID { get; private set; } = 0;
    public static int BedrockID { get; private set; } = 69;
    public static int StoneID { get; private set; } = 40;
    public static int DirtID { get; private set; } = 53;
    public static int GrassID { get; private set; } = 9;  
    public static int SandID { get; private set; } = 58;
    public static int SandstoneID { get; private set; } = 62;
    public static int LogID { get; private set; } = 2;
    public static int LeavesID { get; private set; } = 77;
    public static int PlankID { get; private set; } = 10;
    public static int CobblestoneID { get; private set; } = 87;
    public static int BrickID { get; private set; } = 1;
    public static int GlassID { get; private set; } = 88;
    public static int WaterID { get; private set; } = 55; 

    private static Dictionary<string, int> _registry = new Dictionary<string, int>();
    public static BlockData Air => new BlockData(AirID, "Air");
    public static BlockData Bedrock => new BlockData(BedrockID, "Bedrock");
    public static BlockData Stone => new BlockData(StoneID, "Stone");
    public static BlockData Dirt => new BlockData(DirtID, "Dirt");
    public static BlockData Grass => new BlockData(GrassID, "Grass");
    public static BlockData Sand => new BlockData(SandID, "Sand");
    public static BlockData Sandstone => new BlockData(SandstoneID, "Sandstone");
    public static BlockData Log => new BlockData(LogID, "Log");
    public static BlockData Leaves => new BlockData(LeavesID, "Leaves");
    public static BlockData Plank => new BlockData(PlankID, "Plank");
    public static BlockData Cobblestone => new BlockData(CobblestoneID, "Cobblestone");
    public static BlockData Brick => new BlockData(BrickID, "Brick");
    public static BlockData Glass => new BlockData(GlassID, "Glass");
    public static BlockData Water => new BlockData(WaterID, "Water");

    public static void Initialize(TextureBlockConfigData config)
    {
        if (config?.blocks == null)
        {
            Debug.LogWarning("[BlockTypes] No block config loaded");
            return;
        }

        _registry.Clear();
        foreach (BlockEntry b in config.blocks)
            if (!_registry.ContainsKey(b.name)) 
                _registry[b.name] = b.id;

        AirID = Get("Air", AirID);
        BedrockID = Get("Bedrock", BedrockID);
        StoneID = Get("Stone", StoneID);
        DirtID = Get("Dirt", DirtID);
        GrassID = Get("Grass", GrassID);
        SandID = Get("Sand", SandID);
        SandstoneID = Get("Sandstone", SandstoneID);
        LogID = Get("Log", LogID);
        LeavesID = Get("Leaves", LeavesID);
        PlankID = Get("Plank", PlankID);
        CobblestoneID = Get("Cobblestone", CobblestoneID);
        BrickID = Get("Brick", BrickID);
        GlassID = Get("Glass", GlassID);
        WaterID = Get("Water", WaterID);

        TransparentBlocks.Clear();
        foreach (BlockEntry b in config.blocks)
            if (b.transparent)
                TransparentBlocks.Add(b.id);

        Debug.Log($"[BlockTypes] Initialized {_registry.Count} blocks");
    }

    public static int GetIDByName(string name, int fallback = 0)
        => _registry.TryGetValue(name, out int id) ? id : fallback;

    private static int Get(string name, int fallback)
        => _registry.TryGetValue(name, out int id) ? id : fallback;
}