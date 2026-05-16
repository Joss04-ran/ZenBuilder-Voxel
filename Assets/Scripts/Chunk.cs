using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{
    // ── Constants ─────────────────────────────────────────────
    private const int ChunkWidth = 16;
    private const int ChunkHeight = 32;

    // ── Unity Components ──────────────────────────────────────
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private MeshCollider _meshCollider;

    // ── Water Sub-object ──────────────────────────────────────
    [Header("Water Materials")]
    public Material waterSurfaceMaterial;  // Texture-based top face
    public Material waterInteriorMaterial; // Transparent URP for inner faces

    private MeshFilter _wFilter;
    private MeshRenderer _wRenderer;

    // ── Voxel Data ────────────────────────────────────────────
    private int[,,] _voxelMap;
    private byte[,,] _waterMap;

    // ── Opaque Mesh Buffers ───────────────────────────────────
    private readonly List<Vector3> _verts = new List<Vector3>();
    private readonly List<int> _tris = new List<int>();
    private readonly List<Vector2> _uvs = new List<Vector2>();
    private int _vi;

    // ── Water Surface Buffers (top faces) ─────────────────────
    private readonly List<Vector3> _wSurfVerts = new List<Vector3>();
    private readonly List<int> _wSurfTris = new List<int>();
    private readonly List<Vector2> _wSurfUVs = new List<Vector2>();
    private int _wSi;

    // ── Water Interior Buffers (side + bottom faces) ──────────
    private readonly List<Vector3> _wIntVerts = new List<Vector3>();
    private readonly List<int> _wIntTris = new List<int>();
    private readonly List<Vector2> _wIntUVs = new List<Vector2>();
    private int _wIi;

    // ── Public State ──────────────────────────────────────────
    public Vector2Int ChunkCoord { get; private set; }
    public ChunkFaceVisibility Visibility { get; private set; }
    public LODLevel CurrentLOD { get; private set; } = LODLevel.Full;

    // ── Dependencies ──────────────────────────────────────────
    private TerrainGenerator _terrain;
    private StructureGenerator _structures;
    private static readonly WaterSimulator _waterSim = new WaterSimulator();

    public void Init(Vector2Int coord, int[,,] savedVoxels, byte[,,] savedWater,
        TerrainGenerator terrain = null)
    {
        ChunkCoord = coord;
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshCollider = GetComponent<MeshCollider>();
        transform.position = new Vector3(coord.x * ChunkWidth, 0, coord.y * ChunkWidth);

        _terrain = terrain ?? new TerrainGenerator();
        _structures = new StructureGenerator(_terrain);

        EnsureWaterObject();

        if (savedVoxels != null)
        {
            _voxelMap = (int[,,])savedVoxels.Clone();
            _waterMap = savedWater != null
                ? (byte[,,])savedWater.Clone()
                : new byte[ChunkWidth, ChunkHeight, ChunkWidth];
        }
        else
        {
            PopulateVoxelMap();
        }
        Visibility = ChunkVisibilityComputer.Compute(_voxelMap, ChunkWidth, ChunkHeight);
        UpdateChunk();
    }

    public void Init(Vector2Int coord, int[,,] savedVoxels, TerrainGenerator terrain = null)
        => Init(coord, savedVoxels, null, terrain);

    private void EnsureWaterObject()
    {
        if (_wFilter != null) return;

        var wObj = new GameObject("WaterMesh");
        wObj.transform.SetParent(transform);
        wObj.transform.localPosition = Vector3.zero;
        _wFilter = wObj.AddComponent<MeshFilter>();
        _wRenderer = wObj.AddComponent<MeshRenderer>();
    }

    private void PopulateVoxelMap()
    {
        _voxelMap = new int[ChunkWidth, ChunkHeight, ChunkWidth];
        _waterMap = new byte[ChunkWidth, ChunkHeight, ChunkWidth];
        _terrain.GenerateTerrain(_voxelMap, _waterMap, ChunkCoord, ChunkWidth, ChunkHeight);
        _structures.GenerateStructures(_voxelMap, ChunkCoord, ChunkWidth, ChunkHeight);
    }

    public void SetLOD(LODLevel lod)
    {
        if (lod == CurrentLOD) return;
        CurrentLOD = lod;
        UpdateChunk();
    }
    public void SetVisible(bool visible)
    {
        _meshRenderer.enabled = visible;
        if (_wRenderer != null) _wRenderer.enabled = visible;
    }

    public void EditVoxel(Vector3 pos, int newBlockID)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);
        if (!IsInBounds(x, y, z)) return;

        _voxelMap[x, y, z] = newBlockID;
        if (newBlockID != BlockTypes.AirID) _waterMap[x, y, z] = 0;
        Visibility = ChunkVisibilityComputer.Compute(_voxelMap, ChunkWidth, ChunkHeight);
        UpdateChunk();
    }

    public void PlaceWaterSource(Vector3 localPos)
    {
        int x = Mathf.FloorToInt(localPos.x);
        int y = Mathf.FloorToInt(localPos.y);
        int z = Mathf.FloorToInt(localPos.z);
        if (!IsInBounds(x, y, z) || _voxelMap[x, y, z] != BlockTypes.AirID) return;
        _waterMap[x, y, z] = WaterSimulator.SOURCE;
        UpdateChunk();
    }

    public byte GetWaterLevelAt(Vector3 localPos)
    {
        int x = Mathf.FloorToInt(localPos.x);
        int y = Mathf.FloorToInt(localPos.y);
        int z = Mathf.FloorToInt(localPos.z);
        return IsInBounds(x, y, z) ? _waterMap[x, y, z] : (byte)0;
    }

    public bool StepWater()
        => _waterSim.Tick(_waterMap, _voxelMap, ChunkWidth, ChunkHeight);

    public void UpdateChunk()
    {
        ClearBuffers();
        CreateMeshData();
        BuildMeshes();
    }

    private void ClearBuffers()
    {
        _verts.Clear(); _tris.Clear(); _uvs.Clear(); _vi = 0;
        _wSurfVerts.Clear(); _wSurfTris.Clear(); _wSurfUVs.Clear(); _wSi = 0;
        _wIntVerts.Clear(); _wIntTris.Clear(); _wIntUVs.Clear(); _wIi = 0;
    }

    private void CreateMeshData()
    {
        for (int x = 0; x < ChunkWidth; x++)
            for (int y = 0; y < ChunkHeight; y++)
                for (int z = 0; z < ChunkWidth; z++)
                {
                    if (_voxelMap[x, y, z] != BlockTypes.AirID && ShouldRenderAtLOD(x, y, z))
                        AddOpaqueFaces(x, y, z);

                    if (_waterMap[x, y, z] > 0)
                        AddWaterFaces(x, y, z);
                }
    }
    private bool ShouldRenderAtLOD(int x, int y, int z)
    {
        switch (CurrentLOD)
        {
            case LODLevel.Full:
                return true;

            case LODLevel.Medium:
                return y >= ChunkLODSettings.CaveCutoffY;

            case LODLevel.Far:
                for (int checkY = y + 1; checkY < ChunkHeight; checkY++)
                    if (_voxelMap[x, checkY, z] == BlockTypes.AirID) return true;
                return false;

            default: return true;
        }
    }

    private void AddOpaqueFaces(int x, int y, int z)
    {
        int id = _voxelMap[x, y, z];
        Vector3 pos = new Vector3(x, y, z);

        for (int p = 0; p < 6; p++)
            if (!CheckOpaque(pos + VoxelData.faceChecks[p]))
                AddFace(pos, p, id, _verts, _tris, _uvs, ref _vi);
    }
    private void AddWaterFaces(int x, int y, int z)
    {
        Vector3 pos = new Vector3(x, y, z);

        for (int p = 0; p < 6; p++)
        {
            if (CheckWater(pos + VoxelData.faceChecks[p])) continue;

            bool isTopFace = (p == 2); // Top face index in VoxelData

            if (isTopFace)
                AddFace(pos, p, BlockTypes.WaterID, _wSurfVerts, _wSurfTris, _wSurfUVs, ref _wSi);
            else
                AddFace(pos, p, BlockTypes.WaterID, _wIntVerts, _wIntTris, _wIntUVs, ref _wIi);
        }
    }

    private void AddFace(Vector3 pos, int faceIndex, int blockID,
        List<Vector3> verts, List<int> tris, List<Vector2> uvs, ref int idx)
    {
        verts.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[faceIndex, 0]]);
        verts.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[faceIndex, 1]]);
        verts.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[faceIndex, 2]]);
        verts.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[faceIndex, 3]]);

        tris.Add(idx); tris.Add(idx + 1); tris.Add(idx + 2);
        tris.Add(idx + 2); tris.Add(idx + 1); tris.Add(idx + 3);

        AppendAtlasUVs(blockID, uvs);
        idx += 4;
    }

    private static void AppendAtlasUVs(int blockID, List<Vector2> uvs)
    {
        const int atlasCol = 9, atlasRow = 10;
        int xPos = (blockID - 1) % atlasCol;
        int yPos = atlasRow - 1 - ((blockID - 1) / atlasCol);
        float ux = 1f / atlasCol, uy = 1f / atlasRow;
        float u = xPos * ux, v = yPos * uy;

        uvs.Add(new Vector2(u, v));
        uvs.Add(new Vector2(u, v + uy));
        uvs.Add(new Vector2(u + ux, v));
        uvs.Add(new Vector2(u + ux, v + uy));
    }

    private bool CheckOpaque(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);
        if (!IsInBounds(x, y, z)) return false;
        int id = _voxelMap[x, y, z];
        return id != BlockTypes.AirID && !BlockTypes.IsTransparent(id);
    }

    private bool CheckWater(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);
        if (!IsInBounds(x, y, z)) return false;
        return _voxelMap[x, y, z] != BlockTypes.AirID || _waterMap[x, y, z] > 0;
    }

    private bool IsInBounds(int x, int y, int z)
        => x >= 0 && x < ChunkWidth
        && y >= 0 && y < ChunkHeight
        && z >= 0 && z < ChunkWidth;

    private void BuildMeshes()
    {
        Mesh opaque = new Mesh { name = "ChunkMesh" };
        opaque.SetVertices(_verts);
        opaque.SetTriangles(_tris, 0);
        opaque.SetUVs(0, _uvs);
        opaque.RecalculateNormals();
        _meshFilter.mesh = opaque;
        _meshCollider.sharedMesh = opaque;
        BuildWaterMesh();
    }

    private void BuildWaterMesh()
    {
        var allVerts = new List<Vector3>(_wSurfVerts);
        allVerts.AddRange(_wIntVerts);
        var allUVs = new List<Vector2>(_wSurfUVs);
        allUVs.AddRange(_wIntUVs);
        int offset = _wSurfVerts.Count;
        List<int> intTrisOffset = new List<int>(_wIntTris.Count);
        foreach (int t in _wIntTris) intTrisOffset.Add(t + offset);

        Mesh wMesh = new Mesh { name = "WaterMesh" };
        wMesh.SetVertices(allVerts);
        wMesh.SetUVs(0, allUVs);
        wMesh.subMeshCount = 2;
        wMesh.SetTriangles(_wSurfTris, 0); 
        wMesh.SetTriangles(intTrisOffset, 1); 
        wMesh.RecalculateNormals();

        _wFilter.mesh = wMesh;
        _wRenderer.sharedMaterials = new[]
        {
            waterSurfaceMaterial,
            waterInteriorMaterial
        };
    }

    public int[,,] GetVoxelData() => (int[,,])_voxelMap.Clone();
    public byte[,,] GetWaterData() => (byte[,,])_waterMap.Clone();

    public void SetVoxelData(int[,,] v, byte[,,] w)
    {
        _voxelMap = (int[,,])v.Clone();
        _waterMap = w != null
            ? (byte[,,])w.Clone()
            : new byte[ChunkWidth, ChunkHeight, ChunkWidth];
    }
}