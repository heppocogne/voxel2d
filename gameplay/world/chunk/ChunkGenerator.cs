using Godot;
using System;
using System.Collections.Generic;

public class ChunkGenerator : Node
{
    [Export]
    public float GaussianSigma = 1.0f;
    [Export]
    public int WorldBottom = 64;
    public uint BaseSeed = 0;

    World worldRoot;
    RandomNumberGenerator rng = new RandomNumberGenerator();

    public override void _Ready()
    {
        worldRoot = GetParent<World>();
    }

    enum Biomes
    {
        PLANE,
        DESERT,
        SNOWY_PLANE,
    }

    public Chunk Generate(int chunk, TileSet tileset)
    {
        var heightNoise = new OpenSimplexNoise();
        int s = (int)BaseSeed;
        heightNoise.Seed = s;
        heightNoise.Persistence = 0.9f;
        heightNoise.Period = 64;

        var dirtNoise = new OpenSimplexNoise();

        dirtNoise.Seed = s + 10;
        dirtNoise.Period = 16;

        var bedrockNoise = new OpenSimplexNoise();
        bedrockNoise.Seed = s + 20;
        bedrockNoise.Period = 8;

        var biomeNoise = new OpenSimplexNoise();
        biomeNoise.Seed = s + 30;
        biomeNoise.Persistence = 0.1f;
        biomeNoise.Period = 640;

        var mountainNoise = new OpenSimplexNoise();
        mountainNoise.Seed = s + 40;
        mountainNoise.Persistence = 0.1f;
        mountainNoise.Period = 96;

        rng.Seed = (ulong)(BaseSeed + chunk + 1 << 31);

        uint biomeNoseOffset = rng.Randi() % (uint)biomeNoise.Period;
        uint mountainNoseOffset = rng.Randi() % (uint)mountainNoise.Period;

        int baseHeight = 56;
        int dirtTop = -32;
        int dirtBottom = 10;

        var map = GD.Load<PackedScene>("res://gameplay/world/chunk/chunk.tscn").Instance() as Chunk;
        worldRoot.AddChild(map);
        map.SetTileSet(tileset);
        map.Position = new Vector2(chunk * Chunk.ChunkSize * 16, 0);

        float[] rawHeights = new float[Chunk.ChunkSize];
        for (int x = 0; x < Chunk.ChunkSize; x++)
        {
            float n = heightNoise.GetNoise1d(chunk * Chunk.ChunkSize + x);
            rawHeights[x] = (n + 1) / 2 * (dirtBottom - dirtTop) + baseHeight;
        }

        for (int x = 0; x < Chunk.ChunkSize; x++)
        {
            float biomeValue = biomeNoise.GetNoise1d(chunk * Chunk.ChunkSize + x + biomeNoseOffset);
            Biomes biome;
            if (biomeValue <= -0.15)
                biome = Biomes.SNOWY_PLANE;
            else if (0.15 <= biomeValue)
                biome = Biomes.DESERT;
            else
                biome = Biomes.PLANE;

            int height = (int)Math.Round(rawHeights[x]);
            int dirtHeight = (int)Math.Round((dirtNoise.GetNoise1d(chunk * Chunk.ChunkSize + x)) * 1.5 + 4);
            int bedrockHeight = (int)Math.Round((bedrockNoise.GetNoise1d(chunk * Chunk.ChunkSize + x)) * 4) + 3;

            // grass block, dirt, ,stone bedrock
            /* 0:grass block
             * 1:dirt
             * 2:stone
             * 3:bedrock
             * 4:snow_grass
             * 5:sand
             */

            int grassBlock = worldRoot.FindTileID("grass_block");
            int dirt = worldRoot.FindTileID("dirt");
            int stone = worldRoot.FindTileID("stone");
            int bedrock = worldRoot.FindTileID("bedrock");
            int grassSnow = worldRoot.FindTileID("grass_block_snow");
            int sand = worldRoot.FindTileID("sand");
            int grass = worldRoot.FindTileID("grass");
            int tallGrass = worldRoot.FindTileID("tall_grass");
            int cactus = worldRoot.FindTileID("cactus");
            int deadBush = worldRoot.FindTileID("dead_bush");

            Resource[] oakTrees = {
                GD.Load("res://gameplay/world/templates/oak_tree1.tres"),
                GD.Load("res://gameplay/world/templates/oak_tree2.tres"),
                GD.Load("res://gameplay/world/templates/oak_tree3.tres"),
            };
            Resource[] spruceTrees = {
                GD.Load("res://gameplay/world/templates/spruce_tree1.tres"),
                GD.Load("res://gameplay/world/templates/spruce_tree2.tres"),
                GD.Load("res://gameplay/world/templates/spruce_tree3.tres"),
            };

            System.Collections.Generic.List<int> layer2Tiles = new System.Collections.Generic.List<int>();
            System.Collections.Generic.List<int> layer3Tiles = new System.Collections.Generic.List<int>();
            switch (biome)
            {
                case Biomes.PLANE:
                    float r = rng.Randf();
                    if (0.75 < r)
                        layer3Tiles.Add(tallGrass);
                    else if (0.5 < r)
                        layer3Tiles.Add(grass);
                    layer2Tiles.Add(grassBlock);
                    for (int i = 0; i < dirtHeight - 1; i++)
                        layer2Tiles.Add(dirt);
                    break;
                case Biomes.SNOWY_PLANE:
                    r = rng.Randf();
                    if (0.75 < r)
                        layer3Tiles.Add(grass);
                    layer2Tiles.Add(grassSnow);
                    for (int i = 0; i < dirtHeight - 1; i++)
                        layer2Tiles.Add(dirt);
                    break;
                case Biomes.DESERT:
                    r = rng.Randf();
                    if (r < 0.1)
                    {
                        int c = rng.RandiRange(1, 3);
                        for (int i = 0; i < c; i++)
                            layer3Tiles.Add(cactus);
                    }
                    else if (r < 0.25)
                    {
                        layer3Tiles.Add(deadBush);
                    }
                    for (int i = 0; i < dirtHeight; i++)
                        layer2Tiles.Add(sand);
                    break;
            }

            for (int i = 0; i < height - bedrockHeight - dirtHeight; i++)
                layer2Tiles.Add(stone);
            for (int i = 0; i < bedrockHeight; i++)
                layer2Tiles.Add(bedrock);

            int y = WorldBottom;
            while (layer2Tiles.Count != 0)
            {
                map.SetCell(2, x, y, layer2Tiles[layer2Tiles.Count - 1]);
                layer2Tiles.RemoveAt(layer2Tiles.Count - 1);
                y -= 1;
            }
            if (layer3Tiles.Count == 0)
            {
                if (rng.Randf() < 0.1)
                {
                    switch (biome)
                    {
                        case Biomes.PLANE:
                            oakTrees[GD.Randi() % 3].Call("place_on_tilemap", map.Layers[3], new Vector2(x, y));
                            break;
                        case Biomes.SNOWY_PLANE:
                            spruceTrees[GD.Randi() % 3].Call("place_on_tilemap", map.Layers[3], new Vector2(x, y));
                            break;
                    }
                }
            }
            else
            {
                while (layer3Tiles.Count != 0)
                {
                    map.SetCell(3, x, y, layer3Tiles[layer3Tiles.Count - 1]);
                    layer3Tiles.RemoveAt(layer3Tiles.Count - 1);
                    y -= 1;
                }
            }
        }

        return map;
    }
}
