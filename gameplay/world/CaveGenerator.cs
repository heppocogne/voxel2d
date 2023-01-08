using Godot;
using System;
using System.CodeDom.Compiler;

public class CaveGenerator : Node
{
    [Export]
    public float MinLength = 16;
    [Export]
    public float MaxLength = 256;
    [Export]
    public float MinWidth = 0;
    [Export]
    public float MaxWidth = 4;
    [Export]
    public float Curve = (float)Math.PI / 2;

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
        RandomNumberGenerator rng = new RandomNumberGenerator();
        rng.Seed = (ulong)(BaseSeed + chunk + 1 << 31);

        OpenSimplexNoise directionNoise = new OpenSimplexNoise();
        directionNoise.Seed = (int)BaseSeed;
        directionNoise.Period = 4;
        directionNoise.Persistence = 1.0f;
        OpenSimplexNoise widthNoise = new OpenSimplexNoise();
        widthNoise.Period = 4;
        widthNoise.Seed = (int)BaseSeed + 10;

        Chunk map = loader.GetChunk(chunk);
        Rect2 rect = map.Layers[2].GetUsedRect();
        Vector2 start = new Vector2(rng.RandfRange(0, rect.Size.x), rng.RandfRange(0, rect.Size.y)) + rect.Position + new Vector2(Chunk.ChunkSize, 0) * chunk;

        float currentLength = 0;
        Vector2 currentPosition = start;
        float direction = rng.RandfRange(0, 2 * (float)Math.PI);
        float length = rng.RandfRange(MinLength, MaxWidth);
        while (currentLength < length)
        {
            float width = (widthNoise.GetNoise1d(currentLength) + 1) / 2 * (MaxWidth - MinWidth) + MinWidth;
            float radius = width / 2;

            // square
            int radiusInt = Mathf.RoundToInt(radius);
            for (int x = -radiusInt; x <= radiusInt; x++)
            {
                for (int y = -radiusInt; y <= radiusInt; y++)
                {
                    Vector2 p = new Vector2(x, y) + currentPosition;
                    int currentCell = worldRoot.GetCellv(p);
                    if (currentCell != 3 && currentCell <= 5)
                        worldRoot.SetCellv(p, -1);
                }
            }
            currentPosition += Mathf.Polar2Cartesian(radius, direction);

            // next loop
            direction += directionNoise.GetNoise1d(currentLength) * Curve;
            //direction += rng.RandfRange(-Curve, Curve);
            currentLength += radius / 16;
        }
    }
}
