using Godot;
using System;

public class Character : Node2D
{
    BaseWorldGenerator generator;
    public override void _Ready()
    {

    }

    public override void _Process(float delta)
    {
        if (Input.IsActionPressed("ui_left"))
            Position += new Vector2(100, 0) * delta;
        else if (Input.IsActionPressed("ui_right"))
            Position -= new Vector2(100, 0) * delta;
        else if (Input.IsActionPressed("ui_up"))
            Position += new Vector2(0, 100) * delta;
        else if (Input.IsActionPressed("ui_down"))
            Position -= new Vector2(0, 100) * delta;

        if (generator == null)
        {
            if (GetTree().Root.HasNode("GameScreen/ViewportContainer/Viewport/OverWorldGenerator"))
                generator = GetTree().Root.GetNode<BaseWorldGenerator>("GameScreen/ViewportContainer/Viewport/OverWorldGenerator");
            else
                return;
        }

        generator.Generate((int)(Position.x / Chunk.ChunkSize));
    }
}
