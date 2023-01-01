using Godot;
using Godot.Collections;
using System;

public abstract class BaseWorldGenerator : Node
{
    [Signal]
    delegate void ProgressChanged(float progress);

    [Export]
    public BaseWorldSettings Settings;

    protected Node2D worldRoot;
    protected TileSet tileset;
    protected int step = 0;
    protected Dictionary<int, Chunk> generatedChunks = new Dictionary<int, Chunk>();
    protected OpenSimplexNoise ceilBedrockThickness = new OpenSimplexNoise();
    protected OpenSimplexNoise floorBedrockThickness = new OpenSimplexNoise();

    public override void _Ready()
    {
        ceilBedrockThickness.Seed = Settings.Seed + 10;
        floorBedrockThickness.Seed = Settings.Seed + 20;
    }

    public abstract void Generate(int chunk);
    public void Generate(int from, int to)
    {
        if (from <= to)
        {
            for (int i = from; i <= to; i++)
            {
                Generate(i);
            }
        }
        else
        {
            Generate(to, from);
        }
    }
}
