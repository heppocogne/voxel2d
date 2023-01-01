using Godot;
using System;

public class Character : Node2D
{
    WorldGenerator generator;
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

        if (generator == null)
        {
            if (GetTree().Root.HasNode("GameScreen/ViewportContainer/Viewport/WorldGenerator"))
                generator = GetTree().Root.GetNode<WorldGenerator>("GameScreen/ViewportContainer/Viewport/WorldGenerator");
            else
                return;
        }

        int c = (int)(Position.x / Chunk.ChunkSize);
        generator.Generate(c - 1, c + 1);
    }
}
