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
        int[] heights = new int[SpawnAreaSize];
        int sumHeight = 0;
        for (int x = 0; x < SpawnAreaSize; x++)
        {
            for (int y = generator.WorldBottom; 0 <= spawnChunk.GetCell(1, x, y); y--, heights[x]--) ;
            sumHeight += heights[x];
        }
        float aveHeight = sumHeight / SpawnAreaSize;
        Vector2 spawnPoint = Vector2.Zero;
        float minDiff = Single.MaxValue;
        for (int x = 0; x < SpawnAreaSize; x++)
        {
            float diff = Math.Abs(heights[x] - aveHeight);
            if (diff < minDiff)
            {
                minDiff = diff;
                spawnPoint = new Vector2(x, heights[x] + generator.WorldBottom);
            }
        }

        GD.Print(spawnPoint);
        player.Position = coordinate.MapToWorld(spawnPoint) + Chunk.CellSize / 2;
    }
}
