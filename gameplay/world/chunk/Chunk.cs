using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public class Chunk : Node2D
{
    static public readonly Vector2 CellSize = new Vector2(16, 16);
    static public readonly int ChunkSize = 32;
    static public readonly int LayersCount = 4;

    public int ChunkNumber;
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
        Godot.Collections.Dictionary layersDic = new Godot.Collections.Dictionary();
        foreach (TileMap layer in Layers)
        {
            /*
             * "layers":{
             *      "Layer1":{0:[],1:[],...},
             *      "Layer2":{...}
             * },
             * "entities":[]
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
            layersDic[layer.Name] = idsDic;
        }

        Godot.Collections.Dictionary dic2 = new Godot.Collections.Dictionary();
        dic2["number"] = ChunkNumber;
        dic2["layers"] = layersDic;
        Godot.Collections.Array<Dictionary> entities = new Godot.Collections.Array<Dictionary>();

        foreach (Entity e in GetTree().GetNodesInGroup("Chunk:" + GD.Str(ChunkNumber)))
        {
            entities.Add(e.Serialize());
        }
        dic2["entities"] = entities;
        GD.Print(entities);

        return dic2;
    }

    static public Chunk Deserialize(Godot.Collections.Dictionary dic, World parent)
    {
        Chunk chunk = GD.Load<PackedScene>("res://gameplay/world/chunk/chunk.tscn").Instance<Chunk>();
        parent.AddChild(chunk);

        chunk.ChunkNumber = (int)dic["number"];

        Dictionary layersDic = (Dictionary)dic["layers"];
        foreach (String layerName in layersDic.Keys)
        {
            Godot.Collections.Dictionary layerDataDic = (Dictionary)layersDic[layerName];
            TileMap layer = chunk.GetNode<TileMap>(layerName);
            foreach (int id in layerDataDic.Keys)
            {
                Vector2[] vecs = (Vector2[])layerDataDic[id];
                foreach (Vector2 vec in vecs)
                    layer.SetCellv(vec, id);
            }
        }

        Godot.Collections.Array entities = (Godot.Collections.Array)dic["entities"];
        foreach (Dictionary e in entities)
        {
            Entity.Deserialize(e, parent);
        }

        return chunk;
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
