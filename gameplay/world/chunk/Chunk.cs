using Godot;
using Godot.Collections;
using System;
using System.Diagnostics;

public class Chunk : TileMap
{
    static public readonly int ChunkSize = 32;

    public override void _Ready()
    {
    }

    public Dictionary Serialize()
    {
        Dictionary dic = new Dictionary();
        Dictionary<Vector2, int> cellsDic = new Dictionary<Vector2, int>();
        foreach (var value in GetUsedCells())
        {
            Vector2 cell = (Vector2)value;
            cellsDic[cell] = GetCellv(cell);
        }
        dic["cells"] = cellsDic;

        return dic;
    }

    public void Deserialize(Dictionary dic)
    {
        Dictionary cellsDic = (Dictionary)dic["cells"];
        foreach (var k in cellsDic.Keys)
        {
            Vector2 vec = (Vector2)k;
            int cell = (int)cellsDic[k];
            SetCellv(vec, cell);
        }
    }
}
