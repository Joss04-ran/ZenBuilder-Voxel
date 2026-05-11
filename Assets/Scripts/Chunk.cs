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
    byte[,,] waterMap;  

    List<Vector3> vertices = new List<Vector3>(), wVerts = new List<Vector3>();
    List<int> tris = new List<int>(), wTris = new List<int>();
    List<Vector2> uvs = new List<Vector2>(), wUVs = new List<Vector2>();
    int vi = 0, wvi = 0;

    public Vector2Int chunkCoord;

    public Material waterMaterial;
    private MeshFilter _wFilter;
    private MeshRenderer _wRenderer;

    private TerrainGenerator _terrain;
    private StructureGenerator _structures;
    private static readonly WaterSimulator _waterSim = new WaterSimulator();

    public void Init(Vector2Int coord, int[,,] savedVoxels, byte[,,] savedWater,
        TerrainGenerator terrain = null)
    {
        chunkCoord = coord;
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
        transform.position = new Vector3(coord.x * ChunkWidth, 0, coord.y * ChunkWidth);

        _terrain = terrain ?? new TerrainGenerator();
        _structures = new StructureGenerator(_terrain);

        if (_wFilter == null)
        {
            var wObj = new GameObject("WaterMesh");
            wObj.transform.SetParent(transform);
            wObj.transform.localPosition = Vector3.zero;
            _wFilter = wObj.AddComponent<MeshFilter>();
            _wRenderer = wObj.AddComponent<MeshRenderer>();
            if (waterMaterial != null) _wRenderer.material = waterMaterial;
        }

        if (savedVoxels != null)
        {
            voxelMap = (int[,,])savedVoxels.Clone();
            waterMap = savedWater != null
                ? (byte[,,])savedWater.Clone()
                : new byte[ChunkWidth, ChunkHeight, ChunkWidth];
        }
        else
        {
            PopulateVoxelMap();
        }

        UpdateChunk();
    }
    public void Init(Vector2Int coord, int[,,] savedVoxels, TerrainGenerator terrain = null)
        => Init(coord, savedVoxels, null, terrain);

    void PopulateVoxelMap()
    {
        voxelMap = new int[ChunkWidth, ChunkHeight, ChunkWidth];
        waterMap = new byte[ChunkWidth, ChunkHeight, ChunkWidth];
        _terrain.GenerateTerrain(voxelMap, waterMap, chunkCoord, ChunkWidth, ChunkHeight);
        _structures.GenerateStructures(voxelMap, chunkCoord, ChunkWidth, ChunkHeight);
    }
    public bool StepWater() => _waterSim.Tick(waterMap, voxelMap, ChunkWidth, ChunkHeight);

    public void EditVoxel(Vector3 pos, int newBlockID)
    {
        int x = Mathf.FloorToInt(pos.x), y = Mathf.FloorToInt(pos.y), z = Mathf.FloorToInt(pos.z);
        if (x < 0 || x >= ChunkWidth || y < 0 || y >= ChunkHeight || z < 0 || z >= ChunkWidth) return;
        voxelMap[x, y, z] = newBlockID;
        if (newBlockID != BlockTypes.AirID) waterMap[x, y, z] = 0;
        UpdateChunk();
    }

    public void PlaceWaterSource(Vector3 localPos)
    {
        int x = Mathf.FloorToInt(localPos.x), y = Mathf.FloorToInt(localPos.y), z = Mathf.FloorToInt(localPos.z);
        if (x < 0 || x >= ChunkWidth || y < 0 || y >= ChunkHeight || z < 0 || z >= ChunkWidth) return;
        if (voxelMap[x, y, z] != BlockTypes.AirID) return; 
        waterMap[x, y, z] = WaterSimulator.SOURCE;
        UpdateChunk();
    }

    public byte GetWaterLevelAt(Vector3 localPos)
    {
        int x = Mathf.FloorToInt(localPos.x), y = Mathf.FloorToInt(localPos.y), z = Mathf.FloorToInt(localPos.z);
        if (x < 0 || x >= ChunkWidth || y < 0 || y >= ChunkHeight || z < 0 || z >= ChunkWidth) return 0;
        return waterMap[x, y, z];
    }

    public void UpdateChunk()
    {
        vertices.Clear(); tris.Clear(); uvs.Clear(); vi = 0;
        wVerts.Clear(); wTris.Clear(); wUVs.Clear(); wvi = 0;
        CreateMeshData();
        CreateMesh();
    }

    void CreateMeshData()
    {
        for (int x = 0; x < ChunkWidth; x++)
            for (int y = 0; y < ChunkHeight; y++)
                for (int z = 0; z < ChunkWidth; z++)
                {
                    if (voxelMap[x, y, z] != BlockTypes.AirID)
                        AddOpaqueFaces(new Vector3(x, y, z));

                    if (waterMap[x, y, z] > 0)
                        AddWaterFaces(new Vector3(x, y, z));
                }
    }

    void AddOpaqueFaces(Vector3 pos)
    {
        int id = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];
        for (int p = 0; p < 6; p++)
            if (!CheckOpaque(pos + VoxelData.faceChecks[p]))
                AddFace(pos, p, id, vertices, tris, uvs, ref vi);
    }
    void AddWaterFaces(Vector3 pos)
    {
        int y = (int)pos.y;
        int wlvl = waterMap[(int)pos.x, y, (int)pos.z];
        int id = GetWaterIDByY(y);

        for (int p = 0; p < 6; p++)
            if (!CheckWater(pos + VoxelData.faceChecks[p]))
                AddFace(pos, p, id, wVerts, wTris, wUVs, ref wvi);
    }
    private int GetWaterIDByY(int y)
    {
        if (y >= 7) return BlockTypes.WaterID;
        if (y >= 4) return BlockTypes.WaterMidID;
        return BlockTypes.WaterDeepID;
    }

    void AddFace(Vector3 pos, int p, int blockID,
        List<Vector3> vList, List<int> tList, List<Vector2> uList, ref int idx)
    {
        vList.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]]);
        vList.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]]);
        vList.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]]);
        vList.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]]);

        tList.Add(idx); tList.Add(idx + 1); tList.Add(idx + 2);
        tList.Add(idx + 2); tList.Add(idx + 1); tList.Add(idx + 3);

        int atlasCol = 9, atlasRow = 10;
        int xPos = (blockID - 1) % atlasCol;
        int yPos = atlasRow - 1 - ((blockID - 1) / atlasCol);
        float ux = 1f / atlasCol, uy = 1f / atlasRow;
        float u = xPos * ux, v = yPos * uy;

        uList.Add(new Vector2(u, v));
        uList.Add(new Vector2(u, v + uy));
        uList.Add(new Vector2(u + ux, v));
        uList.Add(new Vector2(u + ux, v + uy));

        idx += 4;
    }
    bool CheckOpaque(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x), y = Mathf.FloorToInt(pos.y), z = Mathf.FloorToInt(pos.z);
        if (x < 0 || x >= ChunkWidth || y < 0 || y >= ChunkHeight || z < 0 || z >= ChunkWidth) return false;
        int id = voxelMap[x, y, z];
        return id != BlockTypes.AirID && !BlockTypes.IsTransparent(id);
    }
    bool CheckWater(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x), y = Mathf.FloorToInt(pos.y), z = Mathf.FloorToInt(pos.z);
        if (x < 0 || x >= ChunkWidth || y < 0 || y >= ChunkHeight || z < 0 || z >= ChunkWidth) return false;
        return voxelMap[x, y, z] != BlockTypes.AirID || waterMap[x, y, z] > 0;
    }

    void CreateMesh()
    {
        Mesh opaque = new Mesh();
        opaque.vertices = vertices.ToArray();
        opaque.triangles = tris.ToArray();
        opaque.uv = uvs.ToArray();
        opaque.RecalculateNormals();
        meshFilter.mesh = opaque;
        meshCollider.sharedMesh = opaque;

        Mesh wMesh = new Mesh();
        wMesh.vertices = wVerts.ToArray();
        wMesh.triangles = wTris.ToArray();
        wMesh.uv = wUVs.ToArray();
        wMesh.RecalculateNormals();
        if (_wFilter != null) _wFilter.mesh = wMesh;
    }

    public int[,,] GetVoxelData() => (int[,,])voxelMap.Clone();
    public byte[,,] GetWaterData() => (byte[,,])waterMap.Clone();

    public void SetVoxelData(int[,,] v, byte[,,] w)
    {
        voxelMap = (int[,,])v.Clone();
        waterMap = w != null ? (byte[,,])w.Clone() : new byte[ChunkWidth, ChunkHeight, ChunkWidth];
    }
}