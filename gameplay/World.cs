using Godot;
using System;

public class World : Node2D
{
    [Export]
    public int SpawnAreaSize = 10;

    Player player;
    ChunkManager manager;
    ChunkGenerator generator;
    TileMap coordinate;
    public override void _Ready()
    {
        player = GetNode<Player>("Player");
        manager = GetNode<ChunkManager>("ChunkManager");
        generator = manager.GetNode<ChunkGenerator>("ChunkGenerator");
        coordinate = GetNode<TileMap>("Coordinate");

        Chunk spawnChunk = manager.GetChunk(0);
        int height = generator.WorldBottom;
        for (; 0 <= spawnChunk.GetCell(1, 0, height); height--) ;
        Vector2 spawnPoint = new Vector2(0, height);

        GD.Print(spawnPoint);
        player.Position = coordinate.MapToWorld(spawnPoint) + Chunk.CellSize / 2;
    }
}
