using Godot;
using System;
using System.Collections.Generic;

public class ChunkGenerator : Node
{
    [Export]
    public float GaussianSigma = 1.0f;
    public int BaseSeed = 0;

    Node2D worldRoot;
    double[] filter = new double[3];
    double filterSum;

    public override void _Ready()
    {
        filter[0] = Gauss(0, GaussianSigma);
        filter[1] = Gauss(1, GaussianSigma);
        filter[2] = Gauss(2, GaussianSigma);
        filterSum = filter[0] + 2 * (filter[1] + filter[2]);

        GD.Print("[", filter[0], ",", filter[1], ",", filter[2], "]");
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

    static int UintToInt(uint v)
    {
        return (int)((long)v - 1 << 31);
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
        heightNoise.Seed = BaseSeed;
        heightNoise.Period = 96;

        var dirtNoise = new OpenSimplexNoise();

        dirtNoise.Seed = BaseSeed + 10;
        dirtNoise.Period = 16;

        var bedrockNoise = new OpenSimplexNoise();
        bedrockNoise.Seed = BaseSeed + 20;
        bedrockNoise.Period = 8;

        var biomeNoise = new OpenSimplexNoise();
        biomeNoise.Seed = BaseSeed + 30;
        biomeNoise.Period = 640;

        var mountainNoise = new OpenSimplexNoise();
        mountainNoise.Seed = BaseSeed + 40;
        mountainNoise.Period = 64;

        int averageHeight = 64;
        int dirtTop = -10;
        int dirtBottom = 10;
        int mountinTop = -64;


        var map = GD.Load<PackedScene>("res://gameplay/world/chunk/chunk.tscn").Instance() as Chunk;
        map.TileSet = tileset;
        worldRoot.AddChild(map);
        map.Position = new Vector2(chunk * Chunk.ChunkSize * 16, 0);

        float[] rawHeights = new float[Chunk.ChunkSize + 4];
        for (int x = 0; x < Chunk.ChunkSize + 4; x++)
        {
            if (0.125 < mountainNoise.GetNoise1d(chunk * Chunk.ChunkSize + x))
                rawHeights[x] = (heightNoise.GetNoise1d(chunk * Chunk.ChunkSize + x) + 1) / 2 * (dirtBottom - mountinTop) + averageHeight;
            else
                rawHeights[x] = (heightNoise.GetNoise1d(chunk * Chunk.ChunkSize + x) + 1) / 2 * (dirtBottom - dirtTop) + averageHeight;
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

            System.Collections.Generic.List<int> tiles = new System.Collections.Generic.List<int>();
            switch (biome)
            {
                case Biomes.PLANE:
                    tiles.Add(grassBlock);
                    for (int i = 0; i < dirtHeight - 1; i++)
                        tiles.Add(dirt);
                    break;
                case Biomes.SNOWY_PLANE:
                    tiles.Add(grassSnow);
                    for (int i = 0; i < dirtHeight - 1; i++)
                        tiles.Add(dirt);
                    break;
                case Biomes.DESERT:
                    for (int i = 0; i < dirtHeight; i++)
                        tiles.Add(sand);
                    break;
            }

            for (int i = 0; i < height - bedrockHeight - dirtHeight; i++)
                tiles.Add(stone);
            for (int i = 0; i < bedrockHeight; i++)
                tiles.Add(bedrock);

            int y = 64;
            for (int i = 0; i < height; i++)
            {
                map.SetCell(x, y, tiles[tiles.Count - 1]);
                tiles.RemoveAt(tiles.Count - 1);
                y -= 1;
            }
        }
        return map;
    }
}
