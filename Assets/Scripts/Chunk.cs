using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;

    private const int ChunkWidth = 16;
    private const int ChunkHeight = 32; 

    int[,,] voxelMap;

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    int vertexIndex = 0;

    public Vector2Int chunkCoord;
    private TerrainGenerator terrainGenerator;
    private StructureGenerator structureGenerator;

    public void Init(Vector2Int coord, int[,,] savedData, TerrainGenerator terrain = null)
    {
        chunkCoord = coord;
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();

        transform.position = new Vector3(coord.x * ChunkWidth, 0, coord.y * ChunkWidth);

        terrainGenerator = terrain ?? new TerrainGenerator(0f);

        structureGenerator = new StructureGenerator(terrainGenerator);

        if (savedData != null) SetVoxelData(savedData);
        else PopulateVoxelMap();

        UpdateChunk();
    }

    void PopulateVoxelMap()
    {
        voxelMap = new int[ChunkWidth, ChunkHeight, ChunkWidth];
        terrainGenerator.GenerateTerrain(voxelMap, chunkCoord, ChunkWidth, ChunkHeight);
        structureGenerator.GenerateStructures(voxelMap, chunkCoord, ChunkWidth, ChunkHeight);
    }

    public void EditVoxel(Vector3 pos, int newBlockID)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        if (x >= 0 && x < ChunkWidth && y >= 0 && y < ChunkHeight && z >= 0 && z < ChunkWidth)
        {
            voxelMap[x, y, z] = newBlockID;
            UpdateChunk();
        }
    }
    void UpdateChunk()
    {
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
        vertexIndex = 0;
        CreateMeshData();
        CreateMesh();
    }

    void CreateMeshData()
    {
        for (int x = 0; x < ChunkWidth; x++)
            for (int y = 0; y < ChunkHeight; y++)
                for (int z = 0; z < ChunkWidth; z++)
                    if (voxelMap[x, y, z] != BlockTypes.Air.ID)
                        AddVoxelDataToChunk(new Vector3(x, y, z));
    }

    void AddVoxelDataToChunk(Vector3 pos)
    {
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

        if (x < 0 || x > ChunkWidth - 1 || y < 0 || y > ChunkHeight - 1 || z < 0 || z > ChunkWidth - 1)
            return false;

        return voxelMap[x, y, z] != BlockTypes.Air.ID;
    }

    void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    public int[,,] GetVoxelData() => (int[,,])voxelMap.Clone();
    public void SetVoxelData(int[,,] savedData) { voxelMap = (int[,,])savedData.Clone(); }
}