using Godot;
using System;

public class OreGenerator : Node
{
    [Export]
    public float Coal = 0.05f;
    [Export]
    public float Iron = 0.025f;
    [Export]
    public float Gold = 0.01f;
    [Export]
    public float Diamond = 0.01f;

    public uint BaseSeed = 0;

    World worldRoot;
    ChunkLoader loader;

    public override void _Ready()
    {
        worldRoot = GetParent<World>();
        loader = GetNode<ChunkLoader>("../ChunkLoader");
    }

    public void Generate(int chunk)
    {
        int coalOre = worldRoot.FindTileID("coal_ore");
        int ironOre = worldRoot.FindTileID("iron_ore");
        int goldOre = worldRoot.FindTileID("gold_ore");
        int diamondOre = worldRoot.FindTileID("diamond_ore");

        RandomNumberGenerator rng = new RandomNumberGenerator();
        rng.Seed = BaseSeed;

        Chunk map = loader.GetChunk(chunk);
        Rect2 rect = map.Layers[2].GetUsedRect();
        Vector2 chunkOrigin = new Vector2(Chunk.ChunkSize, 0) * chunk;

        for (int y = (int)rect.Position.y; y < (int)rect.End.y; y++)
        {
            for (int x = (int)rect.Position.x; x < (int)rect.End.x; x++)
            {
                if (rng.Randf() < Coal && worldRoot.GetCell(x + Chunk.ChunkSize * chunk, y) == 2)
                    worldRoot.SetCell(x + Chunk.ChunkSize * chunk, y, coalOre);
            }
        }

        rng.Seed = BaseSeed + 10;
        for (int y = 0; y < (int)rect.End.y; y++)
        {
            for (int x = (int)rect.Position.x; x < (int)rect.End.x; x++)
            {
                if (rng.Randf() < Iron && worldRoot.GetCell(x + Chunk.ChunkSize * chunk, y) == 2)
                    worldRoot.SetCell(x + Chunk.ChunkSize * chunk, y, ironOre);
            }
        }

        rng.Seed = BaseSeed + 20;
        for (int y = 32; y < (int)rect.End.y; y++)
        {
            for (int x = (int)rect.Position.x; x < (int)rect.End.x; x++)
            {
                if (rng.Randf() < Gold && worldRoot.GetCell(x + Chunk.ChunkSize * chunk, y) == 2)
                    worldRoot.SetCell(x + Chunk.ChunkSize * chunk, y, goldOre);
            }
        }

        rng.Seed = BaseSeed + 30;
        for (int y = 48; y < (int)rect.End.y; y++)
        {
            for (int x = (int)rect.Position.x; x < (int)rect.End.x; x++)
            {
                if (rng.Randf() < Diamond && worldRoot.GetCell(x + Chunk.ChunkSize * chunk, y) == 2)

                {
                    GD.Print(x, ",", y);
                    worldRoot.SetCell(x + Chunk.ChunkSize * chunk, y, diamondOre);
                }

            }
        }
    }
}
