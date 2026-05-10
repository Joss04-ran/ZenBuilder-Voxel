using UnityEngine;
using System.Collections.Generic;
public static class OreConfigLoader
{
    private const string ResourcePath = "Data/OreConfig";

    public static List<OreData> Load()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(ResourcePath);

        if (jsonFile == null)
        {
            Debug.LogWarning($"[OreConfigLoader] File not found at Resources/{ResourcePath}");
            return new List<OreData>();
        }

        OreDatabase database = JsonUtility.FromJson<OreDatabase>(jsonFile.text);

        if (database?.ores == null)
        {
            Debug.LogWarning("[OreConfigLoader] JSON parsed but ore list is null or empty");
            return new List<OreData>();
        }

        Debug.Log($"[OreConfigLoader] Loaded {database.ores.Count} ore types");
        return database.ores;
    }
}