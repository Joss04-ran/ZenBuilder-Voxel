using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance { get; private set; }

    public GameObject chunkPrefab;
    public Transform player;
    public float waterTickInterval = 0.4f;

    private int _renderDistance;
    private int _worldBorder;
    private float _waterTimer;

    private Queue<Chunk> chunkPool = new Queue<Chunk>();
    private Dictionary<Vector2Int, (int[,,], byte[,,])> worldData = new Dictionary<Vector2Int, (int[,,], byte[,,])>();
    private Dictionary<Vector2Int, Chunk> loadedChunks = new Dictionary<Vector2Int, Chunk>();
    private bool isGenerating;
    private Vector2Int lastPlayerChunk;
    private TerrainGenerator terrainGenerator;
    private HashSet<Vector2Int> _activeWaterChunks = new HashSet<Vector2Int>();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        WorldSettings cfg = ConfigManager.Instance?.WorldConfig?.world;
        _renderDistance = cfg?.renderDistance ?? 5;
        _worldBorder = cfg?.worldBorder ?? 1875;

        terrainGenerator = new TerrainGenerator();

        for (int i = 0; i < 25; i++)
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
        _waterTimer += Time.deltaTime;
        if (_waterTimer >= waterTickInterval)
        {
            _waterTimer = 0f;
            SimulateWater();
        }
    }
    private void SimulateWater()
    {
        var toRemove = new List<Vector2Int>();

        foreach (Vector2Int coord in _activeWaterChunks)
        {
            if (!loadedChunks.TryGetValue(coord, out Chunk chunk)) { toRemove.Add(coord); continue; }

            bool changed = chunk.StepWater();
            if (changed) chunk.UpdateChunk();
            else toRemove.Add(coord);
        }

        foreach (var c in toRemove) _activeWaterChunks.Remove(c);
    }
    public void MarkWaterDirty(Vector2Int chunkCoord)
    {
        _activeWaterChunks.Add(chunkCoord);
    }
    public byte GetWaterLevelAt(Vector3 worldPos)
    {
        Vector2Int coord = GetChunkCoord(worldPos);
        if (!loadedChunks.TryGetValue(coord, out Chunk chunk)) return 0;
        Vector3 local = chunk.transform.InverseTransformPoint(worldPos);
        return chunk.GetWaterLevelAt(local);
    }

    Chunk GetChunkFromPool()
    {
        if (chunkPool.Count > 0) { var c = chunkPool.Dequeue(); c.gameObject.SetActive(true); return c; }
        return Instantiate(chunkPrefab).GetComponent<Chunk>();
    }

    void ReturnChunkToPool(Chunk chunk)
    {
        chunk.gameObject.SetActive(false);
        chunkPool.Enqueue(chunk);
    }

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
            var c = loadedChunks[coord];
            worldData[coord] = (c.GetVoxelData(), c.GetWaterData());
            ReturnChunkToPool(c);
            loadedChunks.Remove(coord);
        }
    }

    IEnumerator LoadChunksCoroutine()
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

            if (worldData.TryGetValue(coord, out var saved))
                chunk.Init(coord, saved.Item1, saved.Item2, terrainGenerator);
            else
                chunk.Init(coord, null, null, terrainGenerator);

            loadedChunks.Add(coord, chunk);
            yield return null;
        }

        isGenerating = false;
    }

    Vector2Int GetChunkCoord(Vector3 p) =>
        new Vector2Int(Mathf.FloorToInt(p.x / 16f), Mathf.FloorToInt(p.z / 16f));
}