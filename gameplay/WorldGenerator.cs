using Godot;
using System;
using System.Collections.Generic;

public class WorldGenerator : Node
{
    Node2D worldRoot;
    public override void _Ready()
    {
        GD.Randomize();
    }

    System.Collections.Generic.Dictionary<int, Chunk> generatedChunk = new System.Collections.Generic.Dictionary<int, Chunk>();

    public void Generate(int chunk)
    {
        if (generatedChunk.ContainsKey(chunk))
            return;

        if (worldRoot == null)
        {
            if (GetTree().Root.HasNode("GameScreen/ViewportContainer/Viewport/Node2DRoot"))
                worldRoot = GetTree().Root.GetNode<Node2D>("GameScreen/ViewportContainer/Viewport/Node2DRoot");
            else
                return;
        }
        var heightNoise = new OpenSimplexNoise();
        heightNoise.Seed = (int)((long)GD.Randi() - 2 << 31);
        heightNoise.Period = 32;

        var dirtNoise = new OpenSimplexNoise();
        dirtNoise.Seed = heightNoise.Seed + 10;
        dirtNoise.Period = 16;

        var bedrockNoise = new OpenSimplexNoise();
        bedrockNoise.Seed = heightNoise.Seed + 20;
        bedrockNoise.Period = 8;

        int average_height = 64;
        int dirtTop = -10;
        int dirtBottom = 10;


        var map = GD.Load<PackedScene>("res://gameplay/world/chunk/chunk.tscn").Instance() as Chunk;
        map.TileSet = GD.Load<TileSet>("res://gameplay/world/overworld_tileset.tres");
        worldRoot.AddChild(map);
        map.Position = new Vector2(chunk * Chunk.ChunkSize * 16, 0);

        for (int x = 0; x < 32; x++)
        {
            int height = (int)Math.Round((heightNoise.GetNoise1d(chunk * Chunk.ChunkSize + x) + 1) / 2 * (dirtBottom - dirtTop)) + average_height;
            //GD.Print(heightNoise.GetNoise1d(chunk * Chunk.ChunkSize + x));
            int dirtHeight = (int)Math.Round((dirtNoise.GetNoise1d(chunk * Chunk.ChunkSize + x)) * 1.5 + 4);
            //if (dirtHeight < 2)
            //    dirtHeight = 2;
            int bedrockHeight = (int)Math.Round((bedrockNoise.GetNoise1d(chunk * Chunk.ChunkSize + x)) * 4) + 3;
            //if (bedrockHeight < 1)
            //    bedrockHeight = 1;

            // grass block, dirt, ,stone bedrock
            /* 0:grass block
             * 1:dirt
             * 2:stone
             * 3:bedrock
             */
            System.Collections.Generic.List<int> blocks = new System.Collections.Generic.List<int>();
            blocks.Add(0);
            for (int i = 0; i < dirtHeight - 1; i++)
                blocks.Add(1);
            for (int i = 0; i < height - bedrockHeight - dirtHeight; i++)
                blocks.Add(2);
            for (int i = 0; i < bedrockHeight; i++)
                blocks.Add(3);

            int y = 64;
            for (int i = 0; i < height; i++)
            {
                map.SetCell(x, y, blocks[blocks.Count - 1]);
                blocks.RemoveAt(blocks.Count - 1);
                y -= 1;
            }
        }
        generatedChunk.Add(chunk, map);
    }

    public void Generate(int from, int to)
    {
        if (to < from)
        {
            Generate(to, from);
            return;
        }
        else
        {
            for (int i = from; i <= to; i++)
                Generate(i);
        }
    }
}
