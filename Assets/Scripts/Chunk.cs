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
    List<Vector2> uvs = new List<Vector2>(); 
    int vertexIndex = 0;
    public Vector2Int chunkCoord;
    public int logBlockID = 7;   
    public int leafBlockID = 8;   
    [Range(0f, 1f)]
    public float treeChance = 0.05f; 
    public void Init(Vector2Int coord)
    {
        chunkCoord = coord;
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();

        transform.position = new Vector3(coord.x * width, 0, coord.y * width);

        PopulateVoxelMap();
        UpdateChunk();
    }
    public float noiseScale = 0.1f; 
    public int maxTerrainHeight = 10; 
    public int solidGroundHeight = 2;

    void PlaceTrees()
    {
        for (int x = 2; x < width - 2; x++)   
        {
            for (int z = 2; z < width - 2; z++)
            {
                int surfaceY = -1;
                for (int y = height - 1; y >= 0; y--)
                {
                    if (voxelMap[x, y, z] != 0)
                    {
                        surfaceY = y;
                        break;
                    }
                }

                if (surfaceY < 0) continue;

                if (Random.value < treeChance)
                    PlaceSingleTree(x, surfaceY + 1, z);
            }
        }
    }

    void PlaceSingleTree(int x, int baseY, int z)
    {
        int trunkHeight = Random.Range(4, 7); 

        for (int y = 0; y < trunkHeight; y++)
        {
            if (baseY + y < height)
                voxelMap[x, baseY + y, z] = logBlockID;
        }


        int leafTop = baseY + trunkHeight;

        for (int ly = leafTop - 2; ly <= leafTop; ly++)
        {
            int radius = (ly == leafTop) ? 1 : 2; 
            for (int lx = -radius; lx <= radius; lx++)
            {
                for (int lz = -radius; lz <= radius; lz++)
                {
                    int tx = x + lx, ty = ly, tz = z + lz;
                    if (tx >= 0 && tx < width && ty >= 0 && ty < height && tz >= 0 && tz < width)
                    {
                        if (voxelMap[tx, ty, tz] == 0)
                            voxelMap[tx, ty, tz] = leafBlockID;
                    }
                }
            }
        }
    }

    void PopulateVoxelMap()
    {
        voxelMap = new int[width, height, width];

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < width; z++)
            {
                float worldX = (chunkCoord.x * width + x) * noiseScale;
                float worldZ = (chunkCoord.y * width + z) * noiseScale;

                float noiseValue = Mathf.PerlinNoise(worldX, worldZ);
                int surfaceHeight = Mathf.FloorToInt(noiseValue * maxTerrainHeight) + solidGroundHeight;
                surfaceHeight = Mathf.Clamp(surfaceHeight, 0, height - 1);

                for (int y = 0; y < height; y++)
                {
                    if (y == surfaceHeight) voxelMap[x, y, z] = 19;
                    else if (y < surfaceHeight) voxelMap[x, y, z] = 7;
                    else voxelMap[x, y, z] = 0;
                }
            }
        }

        PlaceTrees();
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
        uvs.Clear();
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
        mesh.uv = uvs.ToArray(); 
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }
}