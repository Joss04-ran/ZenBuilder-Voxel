using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance { get; private set; }

    public GameObject chunkPrefab;
    public Transform player;
    public float waterTickInterval = 0.4f;
    public int maxVisibilitySteps = 64;

    private int _renderDistance;
    private int _worldBorder;
    private float _waterTimer;
    private float _visTimer;
    private const float VisUpdateInterval = 0.15f; 

    private Queue<Chunk> chunkPool = new Queue<Chunk>();
    private Dictionary<Vector2Int, (int[,,], byte[,,])> worldData = new Dictionary<Vector2Int, (int[,,], byte[,,])>();
    private Dictionary<Vector2Int, Chunk> loadedChunks = new Dictionary<Vector2Int, Chunk>();
    private HashSet<Vector2Int> _visibleSet = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> _activeWaterChunks = new HashSet<Vector2Int>();

    private bool isGenerating;
    private Vector2Int lastPlayerChunk;
    private TerrainGenerator terrainGenerator;

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

        // Throttled visibility + LOD update
        _visTimer += Time.deltaTime;
        if (_visTimer >= VisUpdateInterval)
        {
            _visTimer = 0f;
            ApplyVisibilityAndLOD();
        }

        // Water simulation
        _waterTimer += Time.deltaTime;
        if (_waterTimer >= waterTickInterval)
        {
            _waterTimer = 0f;
            SimulateWater();
        }
    }
    private void ApplyVisibilityAndLOD()
    {
        Vector2Int playerChunk = GetChunkCoord(player.position);
        Plane[] frustum = GeometryUtility.CalculateFrustumPlanes(Camera.main);

        _visibleSet = ComputeVisibleChunks(playerChunk, frustum);

        foreach (KeyValuePair<Vector2Int, Chunk> kvp in loadedChunks)
        {
            bool isVisible = _visibleSet.Contains(kvp.Key);
            kvp.Value.SetVisible(isVisible);

            if (!isVisible) continue;

            int dist = ChunkLODSettings.ChunkDistance(kvp.Key, playerChunk);
            LODLevel lod = ChunkLODSettings.GetLevel(dist);
            kvp.Value.SetLOD(lod);
        }
    }

    private HashSet<Vector2Int> ComputeVisibleChunks(Vector2Int playerChunk, Plane[] frustum)
    {
        var visible = new HashSet<Vector2Int>();
        var queue = new Queue<(Vector2Int coord, int entryFace, int steps)>();
        queue.Enqueue((playerChunk, -1, maxVisibilitySteps));

        while (queue.Count > 0)
        {
            var (coord, entryFace, stepsLeft) = queue.Dequeue();
            if (!visible.Add(coord)) continue;

            if (!loadedChunks.TryGetValue(coord, out Chunk chunk)) continue;

            ChunkFaceVisibility vis = chunk.Visibility;
            foreach (int exitFace in ChunkFaceVisibility.HorizontalFaces)
            { 
                if (entryFace != -1 && !vis.CanSeeThrough(entryFace, exitFace)) continue;

                Vector2Int neighbor = new Vector2Int(
                    coord.x + ChunkFaceVisibility.NDX[exitFace],
                    coord.y + ChunkFaceVisibility.NDZ[exitFace]);

                if (!IsInRenderRange(neighbor, playerChunk)) continue;
                if (visible.Contains(neighbor)) continue;

                
                int nextSteps = stepsLeft;
                if (loadedChunks.TryGetValue(neighbor, out Chunk neighborChunk))
                {
                    if (IsFullyUnderground(neighbor))
                        nextSteps -= 3;
                }
                if (nextSteps <= 0) continue;
                if (!IsChunkInFrustum(neighbor, frustum)) continue;

                int neighborEntry = ChunkFaceVisibility.Opposite(exitFace);
                queue.Enqueue((neighbor, neighborEntry, nextSteps));
            }
        }

        return visible;
    }

    private bool IsInRenderRange(Vector2Int coord, Vector2Int playerChunk)
        => Mathf.Abs(coord.x - playerChunk.x) <= _renderDistance
        && Mathf.Abs(coord.y - playerChunk.y) <= _renderDistance;

    private bool IsChunkInFrustum(Vector2Int coord, Plane[] frustum)
    {
        const int ChunkWidth = 16;
        const int ChunkHeight = 32;
        Vector3 worldMin = new Vector3(coord.x * ChunkWidth, 0, coord.y * ChunkWidth);
        Bounds bounds = new Bounds(
            worldMin + new Vector3(ChunkWidth / 2f, ChunkHeight / 2f, ChunkWidth / 2f),
            new Vector3(ChunkWidth, ChunkHeight, ChunkWidth));
        return GeometryUtility.TestPlanesAABB(frustum, bounds);
    }

    private bool IsFullyUnderground(Vector2Int coord)
    {
        return false;
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

        foreach (Vector2Int c in toRemove) _activeWaterChunks.Remove(c);
    }

    public void MarkWaterDirty(Vector2Int chunkCoord) => _activeWaterChunks.Add(chunkCoord);

    public byte GetWaterLevelAt(Vector3 worldPos)
    {
        Vector2Int coord = GetChunkCoord(worldPos);
        if (!loadedChunks.TryGetValue(coord, out Chunk chunk)) return 0;
        return chunk.GetWaterLevelAt(chunk.transform.InverseTransformPoint(worldPos));
    }

    private Chunk GetChunkFromPool()
    {
        if (chunkPool.Count > 0)
        {
            Chunk c = chunkPool.Dequeue();
            c.gameObject.SetActive(true);
            return c;
        }
        return Instantiate(chunkPrefab).GetComponent<Chunk>();
    }

    private void ReturnChunkToPool(Chunk chunk)
    {
        chunk.gameObject.SetActive(false);
        chunkPool.Enqueue(chunk);
    }

    private void UnloadFarChunks()
    {
        Vector2Int pc = GetChunkCoord(player.position);
        List<Vector2Int> toUnload = new List<Vector2Int>();

        foreach (KeyValuePair<Vector2Int, Chunk> kvp in loadedChunks)
        {
            if (Mathf.Abs(kvp.Key.x - pc.x) > _renderDistance + 1 ||
                Mathf.Abs(kvp.Key.y - pc.y) > _renderDistance + 1)
                toUnload.Add(kvp.Key);
        }

        foreach (Vector2Int coord in toUnload)
        {
            Chunk c = loadedChunks[coord];
            worldData[coord] = (c.GetVoxelData(), c.GetWaterData());
            ReturnChunkToPool(c);
            loadedChunks.Remove(coord);
        }
    }

    private IEnumerator LoadChunksCoroutine()
    {
        isGenerating = true;
        Vector2Int pc = GetChunkCoord(player.position);
        List<Vector2Int> toLoad = new List<Vector2Int>();

        for (int x = -_renderDistance; x <= _renderDistance; x++)
            for (int z = -_renderDistance; z <= _renderDistance; z++)
            {
                Vector2Int coord = new Vector2Int(pc.x + x, pc.y + z);
                if (Mathf.Abs(coord.x) > _worldBorder || Mathf.Abs(coord.y) > _worldBorder) continue;
                if (!loadedChunks.ContainsKey(coord)) toLoad.Add(coord);
            }

        toLoad.Sort((a, b) =>
            Vector2Int.Distance(a, pc).CompareTo(Vector2Int.Distance(b, pc)));

        foreach (Vector2Int coord in toLoad)
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

    private Vector2Int GetChunkCoord(Vector3 p)
        => new Vector2Int(Mathf.FloorToInt(p.x / 16f), Mathf.FloorToInt(p.z / 16f));
}