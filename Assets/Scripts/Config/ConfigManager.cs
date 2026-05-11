using UnityEngine;
[DefaultExecutionOrder(-100)]
public class ConfigManager : MonoBehaviour
{
    public static ConfigManager Instance { get; private set; }

    public TextureBlockConfigData BlockConfig { get; private set; }
    public StructureConfigData StructConfig { get; private set; }
    public BiomeConfigData BiomeConfig { get; private set; }
    public WorldGenConfigData WorldConfig { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAll();
        BlockTypes.Initialize(BlockConfig);
    }
    private void LoadAll()
    {
        BlockConfig = Load<TextureBlockConfigData>("Data/TextureBlockConfig");
        StructConfig = Load<StructureConfigData>("Data/StructureConfig");
        BiomeConfig = Load<BiomeConfigData>("Data/BiomeConfig");
        WorldConfig = Load<WorldGenConfigData>("Data/WorldGenConfig");

        Debug.Log($"[ConfigManager] Loaded: " +
            $"{BlockConfig?.blocks?.Count} blocks, " +
            $"{StructConfig?.structures?.Count} structures, " +
            $"{BiomeConfig?.biomes?.Count} biomes, " +
            $"{WorldConfig?.ores?.Count} ores");
    }
    private T Load<T>(string resourcePath) where T : class, new()
    {
        TextAsset asset = Resources.Load<TextAsset>(resourcePath);

        if (asset == null)
        {
            Debug.LogWarning($"[ConfigManager] File not found: Resources/{resourcePath}.json");
            return new T();
        }

        T result = JsonUtility.FromJson<T>(asset.text);

        if (result == null)
        {
            Debug.LogWarning($"[ConfigManager] Failed to parse: {resourcePath}.json");
            return new T();
        }

        return result;
    }
}