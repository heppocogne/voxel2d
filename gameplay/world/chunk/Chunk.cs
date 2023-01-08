using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public class Chunk : Node2D
{
    static public readonly Vector2 CellSize = new Vector2(16, 16);
    static public readonly int ChunkSize = 32;
    static public readonly int LayersCount = 4;

    public List<TileMap> Layers = new List<TileMap>();


    public override void _Ready()
    {
        foreach (TileMap layer in GetChildren())
        {
            Layers.Add(layer);
        }

        if (LayersCount != Layers.Count)
        {
            GD.PushError("LayersCount is wrong");
        }
    }

    public Godot.Collections.Dictionary Serialize()
    {
        Godot.Collections.Dictionary dic = new Godot.Collections.Dictionary();
        foreach (TileMap layer in Layers)
        {
            /*
             * "Layer0":{
             *      0:[...],
             *      1:[...],
             *      ...
             *  },...
             */
            Godot.Collections.Dictionary<int, Vector2[]> idsDic = new Godot.Collections.Dictionary<int, Vector2[]>();
            foreach (int id in layer.TileSet.GetTilesIds())
            {
                var cells = layer.GetUsedCellsById(id);
                Vector2[] vecs = new Vector2[cells.Count];
                for (int i = 0; i < cells.Count; i++)
                    vecs[i] = (Vector2)cells[i];

                idsDic[id] = vecs;
            }
            dic[layer.Name] = idsDic;
        }

        return dic;
    }

    public void Deserialize(Godot.Collections.Dictionary dic)
    {
        foreach (String layerName in dic.Keys)
        {
            Godot.Collections.Dictionary layerDic = (Godot.Collections.Dictionary)dic[layerName];
            TileMap layer = GetNode<TileMap>(layerName);
            foreach (int id in layerDic.Keys)
            {
                Vector2[] vecs = (Vector2[])layerDic[id];
                foreach (Vector2 vec in vecs)
                    layer.SetCellv(vec, id);
            }
        }
    }

    public void SetTileSet(TileSet tileset)
    {
        if (!IsInsideTree())
        {
            GD.PushWarning("Cannot set tileset");
        }
        foreach (TileMap layer in Layers)
        {
            layer.TileSet = tileset;
        }
    }

    public void SetCellv(int layer, Vector2 cell, int tile)
    {
        for (int i = 0; i < Layers.Count; i++)
        {
            if (layer == i)
                Layers[i].SetCellv(cell, tile);
            else
                Layers[i].SetCellv(cell, -1);
        }
    }

    public void SetCell(int layer, int x, int y, int tile)
    {
        for (int i = 0; i < Layers.Count; i++)
        {
            if (layer == i)
                Layers[i].SetCell(x, y, tile);
            else
                Layers[i].SetCell(x, y, -1);
        }
    }

    public int GetCellv(Vector2 cell)
    {
        foreach (TileMap l in Layers)
        {
            if (l.GetCellv(cell) != -1)
                return l.GetCellv(cell);
        }
        return -1;
    }

    public int GetCell(int x, int y)
    {
        foreach (TileMap l in Layers)
        {
            if (l.GetCell(x, y) != -1)
                return l.GetCell(x, y);
        }
        return -1;
    }

    public int GetCell(int layer, int x, int y)
    {
        return Layers[layer].GetCell(x, y);
    }
}
