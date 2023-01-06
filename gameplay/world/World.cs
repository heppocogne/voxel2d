using Godot;
using Godot.Collections;
using System;

public class World : Node2D
{
    [Export]
    public int SpawnAreaSize = 10;
    [Export]
    public String WorldName = "";
    [Export]
    public TileSet Tileset;
    [Export]
    Resource _tiledata;
    [Export]
    Resource _itemdata;

    public Godot.Collections.Array Tiledata;
    public Godot.Collections.Array Itemdata;

    Player player;
    ChunkLoader loader;
    ChunkGenerator generator;
    TileMap coordinate;
    public override void _Ready()
    {
        player = GetNode<Player>("Player");
        loader = GetNode<ChunkLoader>("ChunkLoader");
        generator = GetNode<ChunkGenerator>("ChunkGenerator");
        coordinate = GetNode<TileMap>("Coordinate");

        Chunk spawnChunk = loader.GetChunk(0);
        int height = generator.WorldBottom;
        for (; 0 <= spawnChunk.GetCell(2, 0, height); height--) ;
        Vector2 spawnPoint = new Vector2(0, height);

        player.Position = coordinate.MapToWorld(spawnPoint) + Chunk.CellSize / 2;

        Tiledata = (Godot.Collections.Array)_tiledata.Get("records");
        Itemdata = (Godot.Collections.Array)_itemdata.Get("records");
    }

    static public int MapToChunk(int x)
    {
        if (0 <= x || x % Chunk.ChunkSize == 0)
            return x / Chunk.ChunkSize;
        else
            return x / Chunk.ChunkSize - 1;
    }

    public int GetCell(int x, int y)
    {
        Chunk chunk = loader.GetChunk(MapToChunk(x));
        if (0 <= x)
            return chunk.GetCell(x % Chunk.ChunkSize, y);
        else
            return chunk.GetCell(Mathf.PosMod(x, Chunk.ChunkSize), y);
    }

    public int GetUsedLayer(int x, int y)
    {
        Chunk chunk = loader.GetChunk(MapToChunk(x));
        for (int i = 0; i < Chunk.LayersCount; i++)
        {
            int id;
            if (0 <= x)
                id = chunk.GetCell(i, x % Chunk.ChunkSize, y);
            else
                id = chunk.GetCell(i, Mathf.PosMod(x, Chunk.ChunkSize), y);

            if (id != 0)
                return i;
        }
        return -1;
    }

    public int GetCellv(Vector2 cell)
    {
        return GetCell((int)cell.x, (int)cell.y);
    }

    public void SetCell(int x, int y, int tile)
    {
        Chunk chunk = loader.GetChunk(MapToChunk(x));
        if (0 <= x)
        {
            chunk.SetCell(2, x % Chunk.ChunkSize, y, tile);
        }
        else
        {
            chunk.SetCell(2, Mathf.PosMod(x, Chunk.ChunkSize), y, tile);
        }
    }

    public void SetCellv(Vector2 cell, int tile)
    {
        SetCell((int)cell.x, (int)cell.y, tile);
    }

    public int FindTileID(String name)
    {
        if (Tileset == null)
            return -1;

        foreach (int id in Tileset.GetTilesIds())
        {
            if (Tileset.TileGetName(id) == name)
                return id;
        }
        foreach (int id in Tileset.GetTilesIds())
        {
            if (Tileset.TileGetName(id).Contains(name))
                return id;
        }
        return -1;
    }

    public Dictionary FindTileData(String tilename)
    {
        foreach (Dictionary d in Tiledata)
        {
            if ((String)d["Name"] == tilename)
                return d;
        }
        GD.PushError("Tile name " + tilename + " is not found");
        return new Dictionary();
    }

    public Dictionary GetTileData(int id)
    {
        return FindTileData(Tileset.TileGetName(id));
    }

    public Dictionary FindItemData(String itemname)
    {
        foreach (Dictionary d in Itemdata)
        {
            if ((String)d["Name"] == itemname)
                return d;
        }
        GD.PushError("Item name " + itemname + " is not found");
        return new Dictionary();
    }

    public Dictionary GetItemData(String itemname)
    {
        foreach (Dictionary d in Itemdata)
        {
            if ((String)d["Name"] == itemname)
                return d;
        }
        GD.PushError("Item name " + itemname + " is not found");
        return new Dictionary();
    }

    public void OnTileDestroyed(Vector2 cell, String tool)
    {
        int id = GetCellv(cell);
        Dictionary tiledata = GetTileData(id);
        if (tool == "")
        {
            if ((int)(float)tiledata["WithoutTools"] == 1)
            {
                Item item = GD.Load<PackedScene>("res://gameplay/entity/item/item.tscn").Instance() as Item;
                item.ItemTexture = Tileset.TileGetTexture(id);
                item.ItemName = Tileset.TileGetName(id);
                item.Position = coordinate.MapToWorld(cell) + new Vector2((float)GD.RandRange(0, 12), (float)GD.RandRange(0, 12));
                item.Velocity = Mathf.Polar2Cartesian(32, (float)GD.RandRange(Math.PI, 2 * Math.PI));
                AddChild(item);
            }
        }
        SetCellv(cell, -1);
    }
}
