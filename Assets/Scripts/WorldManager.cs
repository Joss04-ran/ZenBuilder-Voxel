using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public GameObject chunkPrefab;
    public Transform player;

    public int renderDistance = 5;
    public int worldBorder = 1875;

    private Queue<Chunk> chunkPool = new Queue<Chunk>();
    public int initialPoolSize = 25; 

    private Dictionary<Vector2Int, int[,,]> worldData = new Dictionary<Vector2Int, int[,,]>();

    private Dictionary<Vector2Int, Chunk> loadedChunks = new Dictionary<Vector2Int, Chunk>();

    private bool isGenerating = false;
    private Vector2Int lastPlayerChunk;


    void Start()
    {
        PrewarmPool();
        lastPlayerChunk = GetChunkCoord(player.position);
        StartCoroutine(LoadChunksCoroutine());
    }

    void Update()
    {
        Vector2Int currentChunk = GetChunkCoord(player.position);

        if (currentChunk != lastPlayerChunk)
        {
            lastPlayerChunk = currentChunk;
            UnloadFarChunks();

            if (!isGenerating)
                StartCoroutine(LoadChunksCoroutine());
        }
    }

    void PrewarmPool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            Chunk c = Instantiate(chunkPrefab).GetComponent<Chunk>();
            c.gameObject.SetActive(false);
            chunkPool.Enqueue(c);
        }
    }

    Chunk GetChunkFromPool()
    {
        if (chunkPool.Count > 0)
        {
            Chunk c = chunkPool.Dequeue();
            c.gameObject.SetActive(true);
            return c;
        }
        Debug.LogWarning("Chunk Pool is Empty! Add initialPoolSize.");
        return Instantiate(chunkPrefab).GetComponent<Chunk>();
    }

    void ReturnChunkToPool(Chunk chunk)
    {
        chunk.gameObject.SetActive(false);
        chunkPool.Enqueue(chunk);
    }

    void UnloadFarChunks()
    {
        Vector2Int playerChunk = GetChunkCoord(player.position);
        List<Vector2Int> toUnload = new List<Vector2Int>();

        foreach (var kvp in loadedChunks)
        {
            int dx = Mathf.Abs(kvp.Key.x - playerChunk.x);
            int dz = Mathf.Abs(kvp.Key.y - playerChunk.y);

            if (dx > renderDistance + 1 || dz > renderDistance + 1)
                toUnload.Add(kvp.Key);
        }

        foreach (Vector2Int coord in toUnload)
        {
            Chunk chunk = loadedChunks[coord];

            worldData[coord] = chunk.GetVoxelData();

            ReturnChunkToPool(chunk); 
            loadedChunks.Remove(coord);
        }
    }


    IEnumerator LoadChunksCoroutine()
    {
        isGenerating = true;
        Vector2Int playerChunk = GetChunkCoord(player.position);


        List<Vector2Int> toLoad = new List<Vector2Int>();

        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            for (int z = -renderDistance; z <= renderDistance; z++)
            {
                Vector2Int coord = new Vector2Int(playerChunk.x + x, playerChunk.y + z);

                if (Mathf.Abs(coord.x) > worldBorder || Mathf.Abs(coord.y) > worldBorder)
                    continue;

                if (!loadedChunks.ContainsKey(coord))
                    toLoad.Add(coord);
            }
        }


        toLoad.Sort((a, b) =>
        {
            float da = Vector2Int.Distance(a, playerChunk);
            float db = Vector2Int.Distance(b, playerChunk);
            return da.CompareTo(db);
        });


        foreach (Vector2Int coord in toLoad)
        {
            if (loadedChunks.ContainsKey(coord)) continue;

            Chunk chunk = GetChunkFromPool();

            if (worldData.ContainsKey(coord))
            {
                chunk.Init(coord, worldData[coord]);
            }
            else
            {
                chunk.Init(coord, null);
            }

            loadedChunks.Add(coord, chunk);

            yield return null;
        }

        isGenerating = false;
    }

    Vector2Int GetChunkCoord(Vector3 worldPosition)
    {
        int cx = Mathf.FloorToInt(worldPosition.x / 16f);
        int cz = Mathf.FloorToInt(worldPosition.z / 16f);
        return new Vector2Int(cx, cz);
    }
}