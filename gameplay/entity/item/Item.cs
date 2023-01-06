using Godot;
using System;

public class Item : Entity
{
    public Texture ItemTexture;
    public String ItemName;
    public int Quantity = 1;

    // physics
    public Vector2 Acceleration;

    public override void _Ready()
    {
        base._Ready();
        GetNode<Sprite>("Sprite").Texture = ItemTexture;
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        Velocity += Acceleration * delta;
        MoveAndSlide(Velocity, Vector2.Up);
    }
}
