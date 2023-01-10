using Godot;
using Godot.Collections;
using System;

public class World : Node2D
{
    const File.CompressionMode compressionMode = File.CompressionMode.Zstd;

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
    public Dictionary<Vector2, Utility> UtilityMapping = new Dictionary<Vector2, Utility>();

    Player player;
    ChunkLoader loader;
    ChunkGenerator generator;
    Coordinate coordinate;
    Image screenShot;
    public override void _Ready()
    {
        //player = GetNode<Player>("Player");
        loader = GetNode<ChunkLoader>("ChunkLoader");
        generator = GetNode<ChunkGenerator>("ChunkGenerator");
        coordinate = GetNode<Coordinate>("Coordinate");

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

    public void NewWorld()
    {
        player = GD.Load<PackedScene>("res://gameplay/entity/character/player/player.tscn").Instance<Player>();
        AddChild(player);
        loader.InitAsNewWorld();
        coordinate.Init();

        Chunk spawnChunk = loader.GetChunk(0);
        Rect2 rect = spawnChunk.Layers[2].GetUsedRect();
        rect.Expand(rect.Position);
        Vector2 spawnPoint = new Vector2(0, rect.Position.y - 1);
        for (int x = 0; x < SpawnAreaSize; x++)
        {
            for (int y = (int)rect.Position.y - 1; spawnChunk.GetCell(2, x, y) < 0; y++)
            {
                if (spawnPoint.y < y - 1)
                {
                    spawnPoint = new Vector2(x, y - 1);
                }
            }
        }

        player.Position = coordinate.MapToWorld(spawnPoint) + Chunk.CellSize / 2;
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
        if (tile == -1)
        {
            for (int i = 0; i < Chunk.LayersCount; i++)
            {
                if (0 <= x)
                {
                    chunk.SetCell(i, x % Chunk.ChunkSize, y, tile);
                }
                else
                {
                    chunk.SetCell(i, Mathf.PosMod(x, Chunk.ChunkSize), y, tile);
                }
            }
        }
        else
        {
            Dictionary tiledata = GetTileData(tile);
            if (0 <= x)
            {
                chunk.SetCell((int)(float)tiledata["Layer"], x % Chunk.ChunkSize, y, tile);
            }
            else
            {
                chunk.SetCell((int)(float)tiledata["Layer"], Mathf.PosMod(x, Chunk.ChunkSize), y, tile);
            }

            switch (tile)
            {
                case 25:    // chest
                    Utility chest = GD.Load<PackedScene>("res://gameplay/entity/utility/chest.tscn").Instance<Utility>();
                    AddChild(chest);
                    chest.Position = coordinate.MapToWorld(new Vector2(x, y));
                    UtilityMapping.Add(new Vector2(x, y), chest);
                    chest.ChunkPosition = Coordinate.MapToChunk(x);
                    chest.AddToGroup("Chunk:" + GD.Str(chest.ChunkPosition));
                    break;
                case 26:    // furnace
                    Utility furnace = GD.Load<PackedScene>("res://gameplay/entity/utility/furnace.tscn").Instance<Utility>();
                    AddChild(furnace);
                    furnace.Position = coordinate.MapToWorld(new Vector2(x, y));
                    UtilityMapping.Add(new Vector2(x, y), furnace);
                    furnace.ChunkPosition = Coordinate.MapToChunk(x);
                    furnace.AddToGroup("Chunk:" + GD.Str(furnace.ChunkPosition));
                    break;
            }
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
            {
                int tileID = FindTileID(itemname);
                if (0 <= tileID)
                    d["Texture"] = Tileset.TileGetTexture(tileID).ResourcePath;
                else if (itemname != "__tile_template")
                    d["Texture"] = "res://assets/GoodVibes/items/" + itemname + ".png";
                return d;
            }
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
                item["Texture"] = Tileset.TileGetTexture(FindTileID(itemname)).ResourcePath;

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
        if (ToolFitness || (int)(float)tiledata["RequireTools"] == 0)
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

        switch (id)
        {
            case 25:    // chest
                Utility chest = UtilityMapping[cell];
                foreach (Item i in chest.Inventory.Items)
                {
                    if (i != null)
                    {
                        Item item = CreateItemInstance(i.ItemName);
                        item.Position = coordinate.MapToWorld(cell) + new Vector2((float)GD.RandRange(0, 12), (float)GD.RandRange(0, 12));
                        item.Velocity = Mathf.Polar2Cartesian(32, (float)GD.RandRange(Math.PI, 2 * Math.PI));
                        AddChild(item);
                    }
                }

                UtilityMapping.Remove(cell);
                chest.QueueFree();
                break;
            case 26:    // furnace
                break;
        }
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

    public void SaveWorld()
    {
        UtilityMapping.Clear();
        loader.UnloadAll();
        String dirPath = "user://worlds/" + WorldName + "/";
        Directory dir = new Directory();
        if (!dir.DirExists(dirPath))
        {
            dir.MakeDirRecursive(dirPath);
        }
        String filename = dirPath + "world";

        File f = new File();
        if (f.OpenCompressed(filename, File.ModeFlags.Write, compressionMode) == Error.Ok)
        {
            f.StoreVar(Serialize());
            f.Close();
        }

        screenShot.SavePng(dirPath + "screenshot.png");
    }

    public Dictionary Serialize()
    {
        Dictionary data = new Dictionary();
        data["player_chunk"] = player.ChunkPosition;
        data["generated_chunks"] = loader.GeneratedChunks;
        //data["instance"] = Filename;
        return data;
    }

    public void LoadWorld()
    {
        String filename = "user://worlds/" + WorldName + "/world";

        File f = new File();
        if (f.OpenCompressed(filename, File.ModeFlags.Read, compressionMode) == Error.Ok)
        {
            Dictionary data = (Dictionary)f.GetVar();
            int playerChunk = (int)data["player_chunk"];
            Godot.Collections.Array generatedChunks = (Godot.Collections.Array)data["generated_chunks"];
            loader.GeneratedChunks = new Godot.Collections.Array<int>();
            foreach (int chunk in generatedChunks)
            {
                loader.GeneratedChunks.Add(chunk);
            }
            f.Close();

            Chunk spawnChunk = loader.GetChunk(playerChunk);
            player = GetNode<Player>("Player");
            loader.InitAsLoadedWorld();
            player.ChunkPosition = playerChunk;
            player.EmitSignal("ChunkChanged", playerChunk, 0);
            coordinate.Init();
        }
    }

    public void OnScreenshotTaken(Image taken)
    {
        screenShot = taken;
        Vector2 s = screenShot.GetSize();
        screenShot.Resize(64, (int)(64 * s.y / s.x));
    }

}
