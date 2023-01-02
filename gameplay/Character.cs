using Godot;
using System;

public class Character : Node2D
{
    public int ChunkPosition = 0;
    ChunkManager manager;
    public override void _Ready()
    {

    }

    public override void _Process(float delta)
    {
        int speed = 640;
        if (Input.IsActionPressed("ui_left"))
            Position -= new Vector2(speed, 0) * delta;
        else if (Input.IsActionPressed("ui_right"))
            Position += new Vector2(speed, 0) * delta;
        else if (Input.IsActionPressed("ui_up"))
            Position -= new Vector2(0, speed) * delta;
        else if (Input.IsActionPressed("ui_down"))
            Position += new Vector2(0, speed) * delta;

        if (manager == null)
        {
            if (GetTree().Root.HasNode("GameScreen/ViewportContainer/Viewport/ChunkManager"))
                manager = GetTree().Root.GetNode<ChunkManager>("GameScreen/ViewportContainer/Viewport/ChunkManager");
            else
                return;
        }

        ChunkPosition = (int)(Position.x / (Chunk.ChunkSize * 16));
    }
}
