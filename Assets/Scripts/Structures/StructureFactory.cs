using UnityEngine;

public static class StructureFactory
{
    public static Structure Create(StructureEntry entry)
    {
        switch (entry.type)
        {
            case "Tree":
                return new TreeStructure(entry.minTrunkHeight, entry.maxTrunkHeight);

            case "Cabin":
                return new CabinStructure(entry.width, entry.depth, entry.wallHeight);

            case "Procedural":
                return new ProceduralStructure(entry);

            default:
                Debug.LogWarning($"[StructureFactory] Unknown type: '{entry.type}'");
                return null;
        }
    }
}