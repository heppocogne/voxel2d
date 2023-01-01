using Godot;
using System;
using System.Diagnostics;
using System.Runtime;

public class OverWorldGenerator : BaseWorldGenerator
{
    OverWorldGenerator() : base()
    {
        Settings = new OverWorldSettings();
    }

    private OpenSimplexNoise dirtSurfaceNoise = new OpenSimplexNoise();
    private OpenSimplexNoise dirtThicknessNoise = new OpenSimplexNoise();

    public override void _Ready()
    {
        Debug.Assert(Settings is OverWorldSettings);
        OverWorldSettings _os = (Settings as OverWorldSettings);
        dirtSurfaceNoise.Seed = _os.Seed;
        dirtThicknessNoise.Seed = _os.Seed - 10;
    }

    public override void Generate(int chunk)
    {
        if (worldRoot == null)
        {
            if (GetTree().Root.HasNode("GameScreen/ViewportContainer/Viewport/Node2DRoot"))
                worldRoot = GetTree().Root.GetNode<Node2D>("GameScreen/ViewportContainer/Viewport/Node2DRoot");
            else
                return;
        }


        /* 0:grass
        * 1:dirt
        * 2:stone
        * 3:bedrock
        */
        int[] heights = new int[Chunk.ChunkSize];
        int[] dirts = new int[Chunk.ChunkSize];
        int[] bedrocks = new int[Chunk.ChunkSize];
        OverWorldSettings _os = (Settings as OverWorldSettings);
        for (int x = 0; x < heights.Length; x++)
        {
            float n = dirtSurfaceNoise.GetNoise1d((chunk * Chunk.ChunkSize + x) / _os.NoiseScale);
            heights[x] = (int)((n + 1) / 2 * (_os.MaxGroundHeight - _os.MinGroundHeight)) + _os.MinGroundHeight;

            n = dirtThicknessNoise.GetNoise1d((chunk * Chunk.ChunkSize + x) / _os.NoiseScale);
            dirts[x] = (int)((n + 1) / 2 * (_os.MaxDirtThickness - _os.MinDirtThickness)) + _os.MinDirtThickness;

            n = floorBedrockThickness.GetNoise1d((chunk * Chunk.ChunkSize + x) / _os.NoiseScale);
            bedrocks[x] = (int)((n + 1) / 2 * (_os.MaxBedrockThickness - _os.MinBedrockThickness)) + _os.MinBedrockThickness;
        }

        if (!generatedChunks.ContainsKey(chunk))
        {
            Chunk map = GD.Load<PackedScene>("res://gameplay/world/chunk/chunk.tscn").Instance() as Chunk;
            map.TileSet = Settings.TileSet;
            for (int rx = 0; rx < Chunk.ChunkSize; rx++)
            {
                int yCount = 0;
                for (int y = _os.MaxHeight - 1; _os.MinHeight <= y; y--, yCount++)
                {

                    if (yCount == heights[rx])
                    {
                        map.SetCell(rx, y, 0);
                        break;
                    }
                    else if (heights[rx] - dirts[rx] <= yCount)
                    {
                        map.SetCell(rx, y, 1);
                    }
                    else if (yCount < bedrocks[rx])
                    {
                        map.SetCell(rx, y, 3);
                    }
                    else
                    {
                        map.SetCell(rx, y, 2);
                    }
                }
                worldRoot.AddChild(map);
                map.Position = new Vector2(Chunk.ChunkSize * chunk, 0);
                generatedChunks.Add(chunk, map);
            }
        }
    }
}