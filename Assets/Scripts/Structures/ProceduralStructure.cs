using UnityEngine;
public class ProceduralStructure : Structure
{
    private StructureEntry _entry;
    private int _bpWidth;
    private int _bpDepth;
    private int _bpHeight;

    public ProceduralStructure(StructureEntry entry)
    {
        _entry = entry;

        if (entry.blueprint?.layers != null && entry.blueprint.layers.Count > 0)
        {
            _bpHeight = entry.blueprint.layers.Count;
            _bpDepth = entry.blueprint.layers[0]?.rows?.Count ?? 0;
            _bpWidth = entry.blueprint.layers[0]?.rows?[0]?.cells?.Count ?? 0;
        }
    }
    public override void Generate(int[,,] voxelMap, int originX, int originY, int originZ,
        int chunkWidth, int chunkHeight)
    {
        if (_entry.blueprint?.layers == null) return;

        for (int ly = 0; ly < _entry.blueprint.layers.Count; ly++)
        {
            BlueprintLayer layer = _entry.blueprint.layers[ly];
            if (layer?.rows == null) continue;

            for (int lz = 0; lz < layer.rows.Count; lz++)
            {
                BlueprintCellRow row = layer.rows[lz];
                if (row?.cells == null) continue;

                for (int lx = 0; lx < row.cells.Count; lx++)
                {
                    string blockName = row.cells[lx];
                    if (string.IsNullOrEmpty(blockName) || blockName == "Air") continue;

                    int blockID = BlockTypes.GetIDByName(blockName, BlockTypes.StoneID);
                    TryPlaceBlock(voxelMap,
                        originX + lx, originY + ly, originZ + lz,
                        blockID, chunkWidth, chunkHeight, true);
                }
            }
        }
    }
    public override bool CanPlace(int[,,] voxelMap, int x, int surfaceY, int z,
        int chunkWidth, int chunkHeight)
    {
        if (_bpWidth == 0 || _bpDepth == 0) return false;
        int buffer = 2;
        return x >= buffer && x + _bpWidth < chunkWidth - buffer &&
               z >= buffer && z + _bpDepth < chunkWidth - buffer;
    }
}