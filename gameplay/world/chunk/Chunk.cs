using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public class Chunk : Node2D
{
    static public readonly int ChunkSize = 32;
    List<TileMap> layers = new List<TileMap>();


    public override void _Ready()
    {
        foreach (TileMap layer in GetChildren())
        {
            layers.Add(layer);
        }
    }

    public Godot.Collections.Dictionary Serialize()
    {
        Godot.Collections.Dictionary dic = new Godot.Collections.Dictionary();
        foreach (TileMap layer in layers)
        {
            Godot.Collections.Dictionary<Vector2, int> cellsDic = new Godot.Collections.Dictionary<Vector2, int>();
            foreach (var value in layer.GetUsedCells())
            {
                Vector2 cell = (Vector2)value;
                cellsDic[cell] = layer.GetCellv(cell);
            }
            dic[layer.Name] = cellsDic;
        }

        return dic;
    }

    public void Deserialize(Godot.Collections.Dictionary dic)
    {
        foreach (String layerName in dic.Keys)
        {
            Godot.Collections.Dictionary layerDic = (Godot.Collections.Dictionary)dic[layerName];
            TileMap layer = GetNode<TileMap>(layerName);
            foreach (var k in layerDic.Keys)
            {
                Vector2 vec = (Vector2)k;
                int cell = (int)layerDic[k];
                layer.SetCellv(vec, cell);
            }
        }
    }

    public void SetTileSet(TileSet tileset)
    {
        if (!IsInsideTree())
        {
            GD.PushWarning("Cannot set tileset");
        }
        foreach (TileMap layer in layers)
        {
            layer.TileSet = tileset;
        }
    }

    public int LayerCount()
    {
        return GetChildCount();
    }

    public void SetCellv(int layer, Vector2 cell, int tile)
    {
        layers[layer].SetCellv(cell, tile);
    }

    public void SetCell(int layer, int x, int y, int tile)
    {
        layers[layer].SetCell(x, y, tile);
    }

    public int GetCellv(int layer, Vector2 cell)
    {
        return layers[layer].GetCellv(cell);
    }

    public int GetCell(int layer, int x, int y)
    {
        return layers[layer].GetCell(x, y);
    }
}
