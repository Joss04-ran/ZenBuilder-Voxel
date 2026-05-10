using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public GameObject chunkPrefab;
    public Transform player;
    private int _renderDistance;
    private int _worldBorder;

    private Queue<Chunk> chunkPool = new Queue<Chunk>();
    private Dictionary<Vector2Int, int[,,]> worldData = new Dictionary<Vector2Int, int[,,]>();
    private Dictionary<Vector2Int, Chunk> loadedChunks = new Dictionary<Vector2Int, Chunk>();
    private bool isGenerating = false;
    private Vector2Int lastPlayerChunk;
    private TerrainGenerator terrainGenerator;

    void Start()
    {
        WorldSettings settings = ConfigManager.Instance?.WorldConfig?.world;
        _renderDistance = settings?.renderDistance ?? 5;
        _worldBorder = settings?.worldBorder ?? 1875;
        int poolSize = 25;

        terrainGenerator = new TerrainGenerator();

        for (int i = 0; i < poolSize; i++)
        {
            Chunk c = Instantiate(chunkPrefab).GetComponent<Chunk>();
            c.gameObject.SetActive(false);
            chunkPool.Enqueue(c);
        }

        lastPlayerChunk = GetChunkCoord(player.position);
        StartCoroutine(LoadChunksCoroutine());
    }

    void Update()
    {
        Vector2Int cur = GetChunkCoord(player.position);
        if (cur != lastPlayerChunk)
        {
            lastPlayerChunk = cur;
            UnloadFarChunks();
            if (!isGenerating) StartCoroutine(LoadChunksCoroutine());
        }
    }

    Chunk GetChunkFromPool()
    {
        if (chunkPool.Count > 0) { var c = chunkPool.Dequeue(); c.gameObject.SetActive(true); return c; }
        return Instantiate(chunkPrefab).GetComponent<Chunk>();
    }

    void ReturnChunkToPool(Chunk chunk) { chunk.gameObject.SetActive(false); chunkPool.Enqueue(chunk); }

    void UnloadFarChunks()
    {
        Vector2Int pc = GetChunkCoord(player.position);
        var toUnload = new List<Vector2Int>();

        foreach (var kvp in loadedChunks)
            if (Mathf.Abs(kvp.Key.x - pc.x) > _renderDistance + 1 ||
                Mathf.Abs(kvp.Key.y - pc.y) > _renderDistance + 1)
                toUnload.Add(kvp.Key);

        foreach (var coord in toUnload)
        {
            worldData[coord] = loadedChunks[coord].GetVoxelData();
            ReturnChunkToPool(loadedChunks[coord]);
            loadedChunks.Remove(coord);
        }
    }

    System.Collections.IEnumerator LoadChunksCoroutine()
    {
        isGenerating = true;
        Vector2Int pc = GetChunkCoord(player.position);
        var toLoad = new List<Vector2Int>();

        for (int x = -_renderDistance; x <= _renderDistance; x++)
            for (int z = -_renderDistance; z <= _renderDistance; z++)
            {
                var coord = new Vector2Int(pc.x + x, pc.y + z);
                if (Mathf.Abs(coord.x) > _worldBorder || Mathf.Abs(coord.y) > _worldBorder) continue;
                if (!loadedChunks.ContainsKey(coord)) toLoad.Add(coord);
            }

        toLoad.Sort((a, b) =>
            Vector2Int.Distance(a, pc).CompareTo(Vector2Int.Distance(b, pc)));

        foreach (var coord in toLoad)
        {
            if (loadedChunks.ContainsKey(coord)) continue;
            Chunk chunk = GetChunkFromPool();
            int[,,] saved = worldData.ContainsKey(coord) ? worldData[coord] : null;
            chunk.Init(coord, saved, terrainGenerator);
            loadedChunks.Add(coord, chunk);
            yield return null;
        }

        isGenerating = false;
    }

    Vector2Int GetChunkCoord(Vector3 pos) =>
        new Vector2Int(Mathf.FloorToInt(pos.x / 16f), Mathf.FloorToInt(pos.z / 16f));
}