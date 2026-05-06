using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public GameObject chunkPrefab;   
    public Transform player;

    public int renderDistance = 5;   
    public int worldBorder = 1875;   


    private Dictionary<Vector2Int, Chunk> loadedChunks = new Dictionary<Vector2Int, Chunk>();

    private Vector2Int lastPlayerChunk;

    void Start()
    {
        lastPlayerChunk = GetChunkCoord(player.position);
        LoadChunksAroundPlayer();
    }

    void Update()
    {
        Vector2Int currentChunk = GetChunkCoord(player.position);

        if (currentChunk != lastPlayerChunk)
        {
            lastPlayerChunk = currentChunk;
            LoadChunksAroundPlayer();
            UnloadFarChunks();
        }
    }
    Vector2Int GetChunkCoord(Vector3 worldPosition)
    {
        int cx = Mathf.FloorToInt(worldPosition.x / 16f);
        int cz = Mathf.FloorToInt(worldPosition.z / 16f);
        return new Vector2Int(cx, cz);
    }

    void LoadChunksAroundPlayer()
    {
        Vector2Int playerChunk = GetChunkCoord(player.position);

        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            for (int z = -renderDistance; z <= renderDistance; z++)
            {
                Vector2Int coord = new Vector2Int(playerChunk.x + x, playerChunk.y + z);

                if (Mathf.Abs(coord.x) > worldBorder || Mathf.Abs(coord.y) > worldBorder)
                    continue;

                // Kalau chunk belum ada, buat baru
                if (!loadedChunks.ContainsKey(coord))
                {
                    GameObject go = Instantiate(chunkPrefab);
                    Chunk chunk = go.GetComponent<Chunk>();
                    chunk.Init(coord); 
                    loadedChunks.Add(coord, chunk);
                }
            }
        }
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
            Destroy(loadedChunks[coord].gameObject);
            loadedChunks.Remove(coord);
        }
    }
}