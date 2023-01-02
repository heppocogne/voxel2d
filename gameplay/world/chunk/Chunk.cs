using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public class Chunk : Node2D
{
    static public readonly Vector2 CellSize = new Vector2(16, 16);
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
