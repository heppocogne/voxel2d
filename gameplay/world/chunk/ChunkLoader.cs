using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Runtime.Remoting.Channels;

public class ChunkLoader : Node
{
    const File.CompressionMode compressionMode = File.CompressionMode.Zstd;

    [Export]
    public int VisibleChunkDistance = 3;
    [Export]
    public NodePath PlayerNodePath;

    Player player;
    public World worldRoot;

    System.Collections.Generic.List<int> generatedChunks = new System.Collections.Generic.List<int>();
    System.Collections.Generic.Dictionary<int, Chunk> loadedChunks = new System.Collections.Generic.Dictionary<int, Chunk>();
    ChunkGenerator chunkGenerator;
    CaveGenerator caveGenerator;
    OreGenerator oreGenerator;
    public override void _Ready()
    {
        worldRoot = GetParent<World>();

        chunkGenerator = GetNode<ChunkGenerator>("../ChunkGenerator");
        chunkGenerator.BaseSeed = GD.Randi();
        caveGenerator = GetNode<CaveGenerator>("../CaveGenerator");
        caveGenerator.BaseSeed = GD.Randi();
        oreGenerator = GetNode<OreGenerator>("../OreGenerator");
        oreGenerator.BaseSeed = GD.Randi();
        player = GetNode<Player>(PlayerNodePath);
        player.Connect("ChunkChanged", this, nameof(OnPlayerChunkChanged));

        String dirPath = GetChunkFilePath(0).Replace("0.chunk", "");
        Directory dir = new Directory();
        if (!dir.DirExists(dirPath))
        {
            dir.MakeDirRecursive(dirPath);
        }
    }

    public void OnPlayerChunkChanged(int newChunk, int oldChyunk)
    {
        for (int i = -VisibleChunkDistance; i <= VisibleChunkDistance; i++)
        {
            ShowChunk(i + player.ChunkPosition);
        }
        foreach (var kv in loadedChunks.ToList())
        {
            if (VisibleChunkDistance < Math.Abs(player.ChunkPosition - kv.Key))
            {
                UnloadChunk(kv.Key);
            }
        }
    }

    public void ShowChunk(int chunk)
    {
        if (!generatedChunks.Contains(chunk))
        {
            GenerateChunk(chunk);
        }
        else if (!loadedChunks.ContainsKey(chunk))
        {
            LoadChunk(chunk);
        }
    }

    String GetChunkFilePath(int chunk)
    {
        return "user://worlds/" + worldRoot.WorldName + "/chunks/" + GD.Str(chunk) + ".chunk";
    }

    public void UnloadChunk(int chunk)
    {
        Dictionary data = loadedChunks[chunk].Serialize();
        File f = new File();
        Error err = f.OpenCompressed(GetChunkFilePath(chunk), File.ModeFlags.Write, compressionMode);
        if (err == Error.Ok)
        {
            f.StoreVar(data);
            f.Close();
            loadedChunks[chunk].QueueFree();
            loadedChunks.Remove(chunk);
        }
        else
            GD.PushError("Failed to save chunk " + GD.Str(chunk) + "; file=" + GetChunkFilePath(chunk) + "; error=" + (int)err);
    }

    public Chunk LoadChunk(int chunk)
    {
        File f = new File();
        Error err = f.OpenCompressed(GetChunkFilePath(chunk), File.ModeFlags.Read, compressionMode);
        if (err == Error.Ok)
        {
            Dictionary data = (Dictionary)f.GetVar();
            var map = GD.Load<PackedScene>("res://gameplay/world/chunk/chunk.tscn").Instance() as Chunk;
            worldRoot.AddChild(map);
            map.SetTileSet(worldRoot.Tileset);
            map.Position = new Vector2(chunk * Chunk.ChunkSize * 16, 0);
            map.Deserialize(data);
            loadedChunks.Add(chunk, map);

            f.Close();

            return map;
        }
        else
        {
            GD.PushError("Failed to save chunk " + GD.Str(chunk) + "; file=" + GetChunkFilePath(chunk) + "; error=" + (int)err);
            return null;
        }
    }

    public Chunk GetChunk(int chunk)
    {
        if (!generatedChunks.Contains(chunk))
        {
            return GenerateChunk(chunk);
        }
        else if (!loadedChunks.ContainsKey(chunk))
            return LoadChunk(chunk);
        else
            return loadedChunks[chunk];
    }

    Chunk GenerateChunk(int chunk)
    {
        Chunk map = chunkGenerator.Generate(chunk, worldRoot.Tileset);
        generatedChunks.Add(chunk);
        loadedChunks.Add(chunk, map);
        // This function may call GenerateChunk() internally
        caveGenerator.Generate(chunk);
        oreGenerator.Generate(chunk);
        return map;
    }
}
