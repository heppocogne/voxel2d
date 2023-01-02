using Godot;
using System;
using System.Collections.Generic;

public class ChunkGenerator : Node
{
    [Export]
    public float GaussianSigma = 1.0f;
    public uint BaseSeed = 0;

    Node2D worldRoot;
    double[] filter = new double[3];
    double filterSum;
    RandomNumberGenerator plantsRng = new RandomNumberGenerator();

    public override void _Ready()
    {
        filter[0] = Gauss(0, GaussianSigma);
        filter[1] = Gauss(1, GaussianSigma);
        filter[2] = Gauss(2, GaussianSigma);
        filterSum = filter[0] + 2 * (filter[1] + filter[2]);

        //GD.Print("[", filter[0], ",", filter[1], ",", filter[2], "]");
    }

    static int FindTileID(TileSet tileset, String name)
    {
        if (tileset == null)
            return -1;

        foreach (int id in tileset.GetTilesIds())
        {
            if (tileset.TileGetName(id) == name)
                return id;
        }
        foreach (int id in tileset.GetTilesIds())
        {
            if (tileset.TileGetName(id).Contains(name))
                return id;
        }
        return -1;
    }

    static double Gauss(float x, float sigma)
    {
        var s22 = 2 * sigma * sigma;
        return Math.Exp(-(x * x / s22)) / Math.Sqrt(Math.PI * s22);
    }

    enum Biomes
    {
        PLANE,
        DESERT,
        SNOWY_PLANE,
    }

    public Chunk Generate(int chunk, TileSet tileset)
    {
        if (worldRoot == null)
        {
            if (GetTree().Root.HasNode("GameScreen/ViewportContainer/Viewport/Node2DRoot"))
                worldRoot = GetTree().Root.GetNode<Node2D>("GameScreen/ViewportContainer/Viewport/Node2DRoot");
            else
                return null;
        }
        var heightNoise = new OpenSimplexNoise();
        int s = (int)BaseSeed;
        heightNoise.Seed = s;
        heightNoise.Period = 96;

        var dirtNoise = new OpenSimplexNoise();

        dirtNoise.Seed = s + 10;
        dirtNoise.Period = 16;

        var bedrockNoise = new OpenSimplexNoise();
        bedrockNoise.Seed = s + 20;
        bedrockNoise.Period = 8;

        var biomeNoise = new OpenSimplexNoise();
        biomeNoise.Seed = s + 30;
        biomeNoise.Period = 640;

        var mountainNoise = new OpenSimplexNoise();
        mountainNoise.Seed = s + 40;
        mountainNoise.Period = 64;

        plantsRng.Seed = (ulong)(BaseSeed + chunk + 1 << 31);

        int averageHeight = 64;
        int dirtTop = -10;
        int dirtBottom = 10;
        int mountainTop = -64;
        int mountainMiddle = -32;

        var map = GD.Load<PackedScene>("res://gameplay/world/chunk/chunk.tscn").Instance() as Chunk;
        worldRoot.AddChild(map);
        map.SetTileSet(tileset);
        map.Position = new Vector2(chunk * Chunk.ChunkSize * 16, 0);

        float[] rawHeights = new float[Chunk.ChunkSize + 4];
        for (int x = 0; x < Chunk.ChunkSize + 4; x++)
        {
            float mn = mountainNoise.GetNoise1d(chunk * Chunk.ChunkSize + x);
            float n = heightNoise.GetNoise1d(chunk * Chunk.ChunkSize + x);
            if (0.125 < mn)
                rawHeights[x] = (n + 1) / 2 * (dirtBottom - mountainTop) + averageHeight;
            else if (0.08 < mn)
                rawHeights[x] = (n + 1) / 2 * (dirtBottom - mountainMiddle) + averageHeight;
            else
                rawHeights[x] = (n + 1) / 2 * (dirtBottom - dirtTop) + averageHeight;
        }

        for (int x = 0; x < Chunk.ChunkSize; x++)
        {
            float biomeValue = biomeNoise.GetNoise1d(chunk * Chunk.ChunkSize + x);
            Biomes biome;
            if (biomeValue <= -0.15)
                biome = Biomes.SNOWY_PLANE;
            else if (0.15 <= biomeValue)
                biome = Biomes.DESERT;
            else
                biome = Biomes.PLANE;

            int height = (int)Math.Round((rawHeights[x] * filter[2] + rawHeights[x + 1] * filter[1] + rawHeights[x + 2] * filter[0] + rawHeights[x + 3] * filter[1] + rawHeights[x + 4] * filter[2]) / filterSum);
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
             * 4:snow_grass
             * 5:sand
             */

            int grassBlock = FindTileID(tileset, "grass_block");
            int dirt = FindTileID(tileset, "dirt");
            int stone = FindTileID(tileset, "stone");
            int bedrock = FindTileID(tileset, "bedrock");
            int grassSnow = FindTileID(tileset, "grass_block_snow");
            int sand = FindTileID(tileset, "sand");
            int grass = FindTileID(tileset, "grass");
            int tallGrass = FindTileID(tileset, "tall_grass");
            int cactus = FindTileID(tileset, "cactus");
            int deadBush = FindTileID(tileset, "dead_bush");

            System.Collections.Generic.List<int> layer1Tiles = new System.Collections.Generic.List<int>();
            System.Collections.Generic.List<int> layer2Tiles = new System.Collections.Generic.List<int>();
            switch (biome)
            {
                case Biomes.PLANE:
                    float r = plantsRng.Randf();
                    if (0.75 < r)
                        layer2Tiles.Add(tallGrass);
                    else if (0.5 < r)
                        layer2Tiles.Add(grass);
                    layer1Tiles.Add(grassBlock);
                    for (int i = 0; i < dirtHeight - 1; i++)
                        layer1Tiles.Add(dirt);
                    break;
                case Biomes.SNOWY_PLANE:
                    r = plantsRng.Randf();
                    if (0.75 < r)
                        layer2Tiles.Add(grass);
                    layer1Tiles.Add(grassSnow);
                    for (int i = 0; i < dirtHeight - 1; i++)
                        layer1Tiles.Add(dirt);
                    break;
                case Biomes.DESERT:
                    r = plantsRng.Randf();
                    if (r < 0.1)
                    {
                        int c = plantsRng.RandiRange(1, 3);
                        for (int i = 0; i < c; i++)
                            layer2Tiles.Add(cactus);
                    }
                    else if (r < 0.25)
                    {
                        layer2Tiles.Add(deadBush);
                    }
                    for (int i = 0; i < dirtHeight; i++)
                        layer1Tiles.Add(sand);
                    break;
            }

            for (int i = 0; i < height - bedrockHeight - dirtHeight; i++)
                layer1Tiles.Add(stone);
            for (int i = 0; i < bedrockHeight; i++)
                layer1Tiles.Add(bedrock);

            int y = 64;
            while (layer1Tiles.Count != 0)
            {
                map.SetCell(1, x, y, layer1Tiles[layer1Tiles.Count - 1]);
                layer1Tiles.RemoveAt(layer1Tiles.Count - 1);
                y -= 1;
            }
            while (layer2Tiles.Count != 0)
            {
                map.SetCell(2, x, y, layer2Tiles[layer2Tiles.Count - 1]);
                layer2Tiles.RemoveAt(layer2Tiles.Count - 1);
                y -= 1;
            }
        }
        return map;
    }
}
