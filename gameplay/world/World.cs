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
    [Export(PropertyHint.File, "*.json")]
    String _recipedata;
    [Export]
    Resource _tooldata;

    public Godot.Collections.Array Tiledata;
    public Godot.Collections.Array Itemdata;
    public Dictionary Recipedata;
    public Godot.Collections.Array ToolMaterialData;

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
        Rect2 rect = spawnChunk.Layers[2].GetUsedRect();
        int height = (int)rect.Position.y;
        for (; spawnChunk.GetCell(2, 0, height) < 0; height++) ;
        Vector2 spawnPoint = new Vector2(0, height - 1);

        player.Position = coordinate.MapToWorld(spawnPoint) + Chunk.CellSize / 2;
        /*
        int height = generator.WorldBottom;
        for (; 0 <= spawnChunk.GetCell(2, 0, height); height--) ;
        Vector2 spawnPoint = new Vector2(0, height);

        player.Position = coordinate.MapToWorld(spawnPoint) + Chunk.CellSize / 2;
        */
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

        ToolMaterialData = (Godot.Collections.Array)_tooldata.Get("records");

    }

    public int GetCell(int x, int y)
    {
        Chunk chunk = loader.GetChunk(Coordinate.MapToChunk(x));
        if (0 <= x)
            return chunk.GetCell(x % Chunk.ChunkSize, y);
        else
            return chunk.GetCell(Mathf.PosMod(x, Chunk.ChunkSize), y);
    }

    public int GetUsedLayer(int x, int y)
    {
        Chunk chunk = loader.GetChunk(Coordinate.MapToChunk(x));
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
        Chunk chunk = loader.GetChunk(Coordinate.MapToChunk(x));
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

    public Dictionary FindToolMaterial(String material)
    {
        foreach (Dictionary d in ToolMaterialData)
        {
            if ((String)d["Material"] == material)
                return d;
        }
        GD.PushError("Tool material '" + material + "' is not found");
        return new Dictionary();
    }

    public Item CreateItemInstance(String itemname, int count = 1)
    {
        Item item;
        if ((String)FindItemData(itemname)["Kind"] == "tile")
        {
            item = GD.Load<PackedScene>("res://gameplay/entity/item/item.tscn").Instance() as Item;
            int id = FindTileID(itemname);
            item.ItemTexture = Tileset.TileGetTexture(id);
            item.ItemName = Tileset.TileGetName(id);
        }
        else
        {
            Dictionary d = FindItemData(itemname);
            if ((String)d["Kind"] == "tool")
            {
                Tool itemAsTool = GD.Load<PackedScene>("res://gameplay/entity/item/tool.tscn").Instance() as Tool;
                itemAsTool.MaxDurability = (int)(float)FindToolMaterial(itemname.Left(itemname.Find("_")))["Durability"];
                itemAsTool.Durability = itemAsTool.MaxDurability;

                item = itemAsTool;
            }
            else
            {
                item = GD.Load<PackedScene>("res://gameplay/entity/item/item.tscn").Instance() as Item;
            }
            item.ItemTexture = GD.Load<Texture>((String)d["Texture"]);
            item.ItemName = itemname;
        }
        item.Quantity = count;
        return item;
    }

    public void OnTileDestroyed(Vector2 cell, bool ToolFitness)
    {
        int id = GetCellv(cell);
        Dictionary tiledata = GetTileData(id);
        if (ToolFitness || (int)(float)tiledata["WithoutTools"] == 1)
        {
            String dropItem = (String)tiledata["DropItem"];
            if (dropItem != "")
            {
                Item item = CreateItemInstance(dropItem);
                item.Position = coordinate.MapToWorld(cell) + new Vector2((float)GD.RandRange(0, 12), (float)GD.RandRange(0, 12));
                item.Velocity = Mathf.Polar2Cartesian(32, (float)GD.RandRange(Math.PI, 2 * Math.PI));
                AddChild(item);
            }
        }
        SetCellv(cell, -1);
    }

    public void OnInventoryOpened()
    {
        Modulate = new Color(0.5f, 0.5f, 0.5f);
        player.BlockUserInput = true;
    }

    public void OnInventoryClosed()
    {
        Modulate = new Color(1, 1, 1);
        player.BlockUserInput = false;
    }
}
