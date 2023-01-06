using Godot;
using System;

public class Item : Entity
{
    public String ItemName;
    public int Quantity = 1;
    public Vector2 Acceleration;

    public override void _Ready()
    {
        base._Ready();
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        Velocity += Acceleration * delta;
        MoveAndSlide(Velocity, Vector2.Up);
    }
}
