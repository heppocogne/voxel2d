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
    public int VisibleChunkDistance = 4;
    [Export]
    public NodePath PlayerNodePath;

    //RandomNumberGenerator rng = new RandomNumberGenerator();
    Character player;
    Node2D worldRoot;

    System.Collections.Generic.List<int> generatedChunks = new System.Collections.Generic.List<int>();
    System.Collections.Generic.Dictionary<int, Chunk> loadedChunks = new System.Collections.Generic.Dictionary<int, Chunk>();
    ChunkGenerator generator;
    public override void _Ready()
    {
        //rng.Randomize();
        generator = GetNode<ChunkGenerator>("ChunkGenerator");
        generator.BaseSeed = GD.Randi();
        player = GetNode<Character>(PlayerNodePath);

        String dirPath = GetChunkFilePath(0).Replace("0.chunk", "");
        Directory dir = new Directory();
        if (!dir.DirExists(dirPath))
        {
            dir.MakeDirRecursive(dirPath);
        }
    }

    public override void _Process(float delta)
    {
        //base._Process(delta);
        for (int i = -VisibleChunkDistance; i <= VisibleChunkDistance; i++)
        {
            ShowChunk(i + player.ChunkPosition);
        }
        foreach (var kv in loadedChunks.ToList())
        {
            if (VisibleChunkDistance < Math.Abs(player.ChunkPosition - kv.Key))
            {
                //GD.Print("unload ", kv.Key);
                UnloadChunk(kv.Key);
            }
        }
    }

    public void ShowChunk(int chunk)
    {
        if (!generatedChunks.Contains(chunk))
        {
            //GD.Print("generate ", chunk);
            var map = generator.Generate(chunk, Tileset);
            generatedChunks.Add(chunk);
            loadedChunks.Add(chunk, map);
        }
        else if (!loadedChunks.ContainsKey(chunk))
        {
            //GD.Print("load ", chunk);
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
            if (GetTree().Root.HasNode("GameScreen/ViewportContainer/Viewport/Node2DRoot"))
                worldRoot = GetTree().Root.GetNode<Node2D>("GameScreen/ViewportContainer/Viewport/Node2DRoot");
            else
                return null;
        }

        File f = new File();
        Error err = f.OpenCompressed(GetChunkFilePath(chunk), File.ModeFlags.Read, compressionMode);
        if (err == Error.Ok)
        {
            Dictionary data = (Dictionary)f.GetVar();
            var map = GD.Load<PackedScene>("res://gameplay/world/chunk/chunk.tscn").Instance() as Chunk;
            map.TileSet = Tileset;
            worldRoot.AddChild(map);
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
}
