using System;
using System.Collections.Generic;
[Serializable] public class BlueprintCellRow { public List<string> cells; }
[Serializable] public class BlueprintLayer { public List<BlueprintCellRow> rows; }
[Serializable] public class StructureBlueprint { public List<BlueprintLayer> layers; }

[Serializable]
public class StructureEntry
{
    public string name;
    public string type;
    public float spawnChance;
    public int[] allowedBiomes;

    // Tree params
    public int minTrunkHeight;
    public int maxTrunkHeight;

    // Cabin params
    public int width;
    public int depth;
    public int wallHeight;

    // Procedural structure blueprint — only used when type = "Procedural"
    public StructureBlueprint blueprint;
}

[Serializable]
public class StructureConfigData
{
    public List<StructureEntry> structures;
}