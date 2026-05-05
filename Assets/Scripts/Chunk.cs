using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;

    int width = 16;
    int height = 16;
    int[,,] voxelMap;

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>(); // TAMBAH INI: Daftar UV untuk Tekstur
    int vertexIndex = 0;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();

        PopulateVoxelMap();
        UpdateChunk();
    }

    // Tambahkan 2 variabel pengaturan ini di bagian atas (di bawah int width = 16;)
    public float noiseScale = 0.1f; // Seberapa "melar" bukitnya. Semakin kecil angkanya, bukit semakin landai.
    public int maxTerrainHeight = 10; // Tinggi maksimal bukit
    public int solidGroundHeight = 2; // Lapisan tanah dasar paling bawah agar tidak ada yang bolong

    void PopulateVoxelMap()
    {
        voxelMap = new int[width, height, width];
        float seedX = Random.Range(0f, 1000f);
        float seedZ = Random.Range(0f, 1000f);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < width; z++)
            {
                float pX = (x + seedX) * noiseScale;
                float pZ = (z + seedZ) * noiseScale;

                float noiseValue = Mathf.PerlinNoise(pX, pZ);

                int surfaceHeight = Mathf.FloorToInt(noiseValue * maxTerrainHeight) + solidGroundHeight;

                surfaceHeight = Mathf.Clamp(surfaceHeight, 0, height - 1);

                for (int y = 0; y < height; y++)
                {
                    if (y == surfaceHeight)
                    {
                        // Surface ID
                        voxelMap[x, y, z] = 16;
                    }
                    else if (y < surfaceHeight)
                    {
                        // Underground ID
                        voxelMap[x, y, z] = 6;
                    }
                    else
                    {
                        // Air
                        voxelMap[x, y, z] = 0;
                    }
                }
            }
        }
    }

    public void EditVoxel(Vector3 pos, int newBlockID)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (x >= 0 && x < width && y >= 0 && y < height && z >= 0 && z < width)
        {
            voxelMap[x, y, z] = newBlockID;
            UpdateChunk();
        }
    }

    void UpdateChunk()
    {
        vertices.Clear();
        triangles.Clear();
        uvs.Clear(); // TAMBAH INI: Bersihkan memori UV lama
        vertexIndex = 0;

        CreateMeshData();
        CreateMesh();
    }

    void CreateMeshData()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < width; z++)
                {
                    if (voxelMap[x, y, z] != 0)
                        AddVoxelDataToChunk(new Vector3(x, y, z));
                }
            }
        }
    }

    void AddVoxelDataToChunk(Vector3 pos)
    {
        // TAMBAH INI: Baca ID blok apa yang sedang kita proses
        int blockID = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];

        for (int p = 0; p < 6; p++)
        {
            if (!CheckVoxel(pos + VoxelData.faceChecks[p]))
            {
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]]);

                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);

                int atlasColumns = 9;  
                int atlasRows = 10;    

                int xPos = (blockID - 1) % atlasColumns;
                int yPos = atlasRows - 1 - ((blockID - 1) / atlasColumns);

               
                float unitX = 1f / atlasColumns;
                float unitY = 1f / atlasRows;

                float u = xPos * unitX;
                float v = yPos * unitY;


                uvs.Add(new Vector2(u, v));                 
                uvs.Add(new Vector2(u, v + unitY));         
                uvs.Add(new Vector2(u + unitX, v));         
                uvs.Add(new Vector2(u + unitX, v + unitY)); 

                vertexIndex += 4;
            }
        }
    }

    bool CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (x < 0 || x > width - 1 || y < 0 || y > height - 1 || z < 0 || z > width - 1)
            return false;

        return voxelMap[x, y, z] != 0;
    }

    void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray(); // TAMBAH INI: Tempelkan UV ke Mesh
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }
}