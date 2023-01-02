using Godot;
using System;

public class Player : KinematicBody2D
{
    [Export]
    public float WalkSpeed = 128;
    [Export]
    public float RunSpeed = 256;

    public int ChunkPosition = 0;
    public override void _Ready()
    {

    }

    public override void _Process(float delta)
    {
        float speed = WalkSpeed;
        if (Input.IsActionPressed("ui_left"))
            MoveAndCollide(new Vector2(-speed, 0) * delta);
        else if (Input.IsActionPressed("ui_right"))
            MoveAndCollide(new Vector2(speed, 0) * delta);
        /*
        else if (Input.IsActionPressed("ui_up"))
            MoveAndCollide(new Vector2(0, -speed) * delta);
        else if (Input.IsActionPressed("ui_down"))
            MoveAndCollide(new Vector2(0, speed) * delta);
        */

        ChunkPosition = (int)(Position.x / (Chunk.ChunkSize * Chunk.CellSize.x));
    }
}
