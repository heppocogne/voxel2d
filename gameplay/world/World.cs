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
    public Resource Tiledata;

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
        for (; 0 <= spawnChunk.GetCell(1, 0, height); height--) ;
        Vector2 spawnPoint = new Vector2(0, height);

        player.Position = coordinate.MapToWorld(spawnPoint) + Chunk.CellSize / 2;
    }

    static public int MapToChunk(int x)
    {
        if (0 <= x)
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
            return chunk.GetCell(x % Chunk.ChunkSize + Chunk.ChunkSize, y);
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
                id = chunk.GetCell(i, x % Chunk.ChunkSize + Chunk.ChunkSize, y);

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
            chunk.SetCell(1, x % Chunk.ChunkSize, y, tile);
        }
        else
        {
            chunk.SetCell(1, x % Chunk.ChunkSize + Chunk.ChunkSize, y, tile);
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

    public Dictionary GetTileData(String tilename)
    {
        foreach (Dictionary d in (Godot.Collections.Array)Tiledata.Get("records"))
        {
            if ((String)d["Name"] == tilename)
                return d;
        }
        return new Dictionary();
    }

    public Dictionary GetTileData(int id)
    {
        return GetTileData(Tileset.TileGetName(id));
    }

    public void OnTileDestroyed(Vector2 cell)
    {
        SetCellv(cell, -1);
    }
}
