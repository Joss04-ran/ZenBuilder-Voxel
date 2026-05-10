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

    // Opaque geometry lists
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    int vertexIndex = 0;

    // Water geometry lists — rendered separately with transparent material
    List<Vector3> wVertices = new List<Vector3>();
    List<int> wTriangles = new List<int>();
    List<Vector2> wUVs = new List<Vector2>();
    int wVertexIndex = 0;

    public Vector2Int chunkCoord;

    // Water mesh is a child GameObject with no collider so players can swim through it.
    // The material assigned here should be URP/Lit with Surface Type set to Transparent.
    public Material waterMaterial;
    private MeshFilter _waterMeshFilter;
    private MeshRenderer _waterMeshRenderer;

    private TerrainGenerator terrainGenerator;
    private StructureGenerator structureGenerator;

    public void Init(Vector2Int coord, int[,,] savedData, TerrainGenerator terrain = null)
    {
        chunkCoord = coord;
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();

        transform.position = new Vector3(coord.x * ChunkWidth, 0, coord.y * ChunkWidth);

        terrainGenerator = terrain ?? new TerrainGenerator();
        structureGenerator = new StructureGenerator(terrainGenerator);

        if (_waterMeshFilter == null)
        {
            GameObject waterObj = new GameObject("WaterMesh");
            waterObj.transform.SetParent(transform);
            waterObj.transform.localPosition = Vector3.zero;
            _waterMeshFilter = waterObj.AddComponent<MeshFilter>();
            _waterMeshRenderer = waterObj.AddComponent<MeshRenderer>();
            if (waterMaterial != null) _waterMeshRenderer.material = waterMaterial;
        }

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
        vertices.Clear(); triangles.Clear(); uvs.Clear(); vertexIndex = 0;
        wVertices.Clear(); wTriangles.Clear(); wUVs.Clear(); wVertexIndex = 0;
        CreateMeshData();
        CreateMesh();
    }

    void CreateMeshData()
    {
        for (int x = 0; x < ChunkWidth; x++)
            for (int y = 0; y < ChunkHeight; y++)
                for (int z = 0; z < ChunkWidth; z++)
                {
                    int id = voxelMap[x, y, z];
                    if (id == BlockTypes.AirID) continue;

                    if (BlockTypes.IsTransparent(id))
                        AddWaterFaces(new Vector3(x, y, z), id);
                    else
                        AddVoxelDataToChunk(new Vector3(x, y, z));
                }
    }
    void AddVoxelDataToChunk(Vector3 pos)
    {
        int blockID = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];

        for (int p = 0; p < 6; p++)
        {
            if (!CheckVoxelOpaque(pos + VoxelData.faceChecks[p]))
                AddFace(pos, p, blockID, vertices, triangles, uvs, ref vertexIndex);
        }
    }
    void AddWaterFaces(Vector3 pos, int blockID)
    {
        for (int p = 0; p < 6; p++)
        {
            if (!CheckVoxelWater(pos + VoxelData.faceChecks[p]))
                AddFace(pos, p, blockID, wVertices, wTriangles, wUVs, ref wVertexIndex);
        }
    }
    void AddFace(Vector3 pos, int p, int blockID,
        List<Vector3> verts, List<int> tris, List<Vector2> uvList, ref int vi)
    {
        verts.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]]);
        verts.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]]);
        verts.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]]);
        verts.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]]);

        tris.Add(vi); tris.Add(vi + 1); tris.Add(vi + 2);
        tris.Add(vi + 2); tris.Add(vi + 1); tris.Add(vi + 3);

        int atlasColumns = 9, atlasRows = 10;
        int xPos = (blockID - 1) % atlasColumns;
        int yPos = atlasRows - 1 - ((blockID - 1) / atlasColumns);
        float unitX = 1f / atlasColumns, unitY = 1f / atlasRows;
        float u = xPos * unitX, v = yPos * unitY;

        uvList.Add(new Vector2(u, v));
        uvList.Add(new Vector2(u, v + unitY));
        uvList.Add(new Vector2(u + unitX, v));
        uvList.Add(new Vector2(u + unitX, v + unitY));

        vi += 4;
    }
    bool CheckVoxelOpaque(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x), y = Mathf.FloorToInt(pos.y), z = Mathf.FloorToInt(pos.z);
        if (x < 0 || x >= ChunkWidth || y < 0 || y >= ChunkHeight || z < 0 || z >= ChunkWidth)
            return false;
        int id = voxelMap[x, y, z];
        return id != BlockTypes.AirID && !BlockTypes.IsTransparent(id);
    }
    bool CheckVoxelWater(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x), y = Mathf.FloorToInt(pos.y), z = Mathf.FloorToInt(pos.z);
        if (x < 0 || x >= ChunkWidth || y < 0 || y >= ChunkHeight || z < 0 || z >= ChunkWidth)
            return false;
        return voxelMap[x, y, z] != BlockTypes.AirID;
    }

    void CreateMesh()
    {
        Mesh opaqueMesh = new Mesh();
        opaqueMesh.vertices = vertices.ToArray();
        opaqueMesh.triangles = triangles.ToArray();
        opaqueMesh.uv = uvs.ToArray();
        opaqueMesh.RecalculateNormals();
        meshFilter.mesh = opaqueMesh;
        meshCollider.sharedMesh = opaqueMesh;

        Mesh waterMesh = new Mesh();
        waterMesh.vertices = wVertices.ToArray();
        waterMesh.triangles = wTriangles.ToArray();
        waterMesh.uv = wUVs.ToArray();
        waterMesh.RecalculateNormals();
        if (_waterMeshFilter != null) _waterMeshFilter.mesh = waterMesh;
    }

    public int[,,] GetVoxelData() => (int[,,])voxelMap.Clone();
    public void SetVoxelData(int[,,] savedData) { voxelMap = (int[,,])savedData.Clone(); }
}