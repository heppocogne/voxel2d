using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Runtime.Remoting.Channels;

public class ChunkManager : Node
{
    const File.CompressionMode compressionMode = File.CompressionMode.Zstd;

    [Export]
    TileSet Tileset;
    [Export]
    public String WorldName = "";
    [Export]
    public int VisibleChunkDistance = 3;
    [Export]
    public NodePath PlayerNodePath;

    Player player;
    Node2D worldRoot;

    System.Collections.Generic.List<int> generatedChunks = new System.Collections.Generic.List<int>();
    System.Collections.Generic.Dictionary<int, Chunk> loadedChunks = new System.Collections.Generic.Dictionary<int, Chunk>();
    ChunkGenerator generator;
    public override void _Ready()
    {
        generator = GetNode<ChunkGenerator>("ChunkGenerator");
        generator.BaseSeed = GD.Randi();
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
            var map = generator.Generate(chunk, Tileset);
            generatedChunks.Add(chunk);
            loadedChunks.Add(chunk, map);
        }
        else if (!loadedChunks.ContainsKey(chunk))
        {
            LoadChunk(chunk);
        }
    }

    String GetChunkFilePath(int chunk)
    {
        return "user://worlds/" + WorldName + "/chunks/" + GD.Str(chunk) + ".chunk";
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
        if (worldRoot == null)
        {
            if (GetTree().Root.HasNode("GameScreen/ViewportContainer/Viewport/World"))
                worldRoot = GetTree().Root.GetNode<Node2D>("GameScreen/ViewportContainer/Viewport/World");
            else
                return null;
        }

        File f = new File();
        Error err = f.OpenCompressed(GetChunkFilePath(chunk), File.ModeFlags.Read, compressionMode);
        if (err == Error.Ok)
        {
            Dictionary data = (Dictionary)f.GetVar();
            var map = GD.Load<PackedScene>("res://gameplay/world/chunk/chunk.tscn").Instance() as Chunk;
            worldRoot.AddChild(map);
            map.SetTileSet(Tileset);
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
            return generator.Generate(chunk, Tileset);
        else if (!loadedChunks.ContainsKey(chunk))
            return LoadChunk(chunk);
        else
            return loadedChunks[chunk];
    }

    static public int ToChunk(int x)
    {
        if (0 <= x)
            return x / Chunk.ChunkSize;
        else
            return x / Chunk.ChunkSize - 1;
    }

    public int GetCell(int x, int y)
    {
        Chunk chunk = GetChunk(ToChunk(x));
        if (0 <= x)
            return chunk.GetCell(x % Chunk.ChunkSize, y);
        else
            return chunk.GetCell(x % Chunk.ChunkSize + Chunk.ChunkSize, y);
    }
}
