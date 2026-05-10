using System;
using System.Collections.Generic;
[Serializable]
public class BlockEntry
{
    public string name;
    public int id;
    public bool transparent;
    public bool isLiquid;
}
[Serializable]
public class TextureBlockConfigData
{
    public List<BlockEntry> blocks;
}