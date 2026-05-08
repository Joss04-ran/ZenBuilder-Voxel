using UnityEngine;

public class StructurePlacementRule
{
    public Structure Structure { get; private set; }
    public float SpawnChance { get; private set; }

    public StructurePlacementRule(Structure structure, float spawnChance)
    {
        Structure = structure;
        SpawnChance = Mathf.Clamp01(spawnChance);
    }
}
