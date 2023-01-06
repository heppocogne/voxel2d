using Godot;
using Godot.Collections;
using System;
using System.Diagnostics;

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
    [Export(PropertyHint.File, "*.json")]
    String _recipedata;

    public Godot.Collections.Array Tiledata;
    public Godot.Collections.Array Itemdata;
    public Dictionary Recipedata;

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

        File f = new File();
        if (f.Open(_recipedata, File.ModeFlags.Read) == Error.Ok)
        {
            var result = JSON.Parse(f.GetAsText());
            if (result.Error == Error.Ok)
            {
                Recipedata = (Dictionary)result.Result;
            }
        }

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
        GD.PushError("Tile name '" + tilename + "' is not found");
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
        // Automatically generate tile items
        Dictionary tiledata = FindTileData(itemname);
        if (tiledata.Contains("Name"))
        {
            Dictionary d = FindItemData("__tile_template");
            Dictionary item = new Dictionary();
            item["Name"] = itemname;
            item["Stack"] = d["Stack"];
            item["Kind"] = d["Kind"];
            if ((String)d["Texture"] != "")
                item["Texture"] = d["Texture"];
            else
                item["Texture"] = Tileset.TileGetTexture(FindTileID(itemname));

            Itemdata.Add(item);
            return item;
        }
        GD.PushError("Item name '" + itemname + "' is not found");
        return new Dictionary();
    }

    public Item CreateItemIntance(String itemname)
    {
        Item item = GD.Load<PackedScene>("res://gameplay/entity/item/item.tscn").Instance() as Item;
        if ((String)FindItemData(itemname)["Kind"] == "tile")
        {
            int id = FindTileID(itemname);
            item.ItemTexture = Tileset.TileGetTexture(id);
            item.ItemName = Tileset.TileGetName(id);
        }
        else
        {
            Dictionary d = FindTileData(itemname);
            item.ItemTexture = GD.Load<Texture>((String)d["Texture"]);
            item.ItemName = itemname;
        }
        return item;
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
