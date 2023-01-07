using Godot;
using System;

public class Item : Entity
{
    public Texture ItemTexture;
    public String ItemName;
    public int Quantity = 1;

    // physics
    public Vector2 Acceleration;
    public float XDamp;

    public override void _Ready()
    {
        base._Ready();
        GetNode<Sprite>("Sprite").Texture = ItemTexture;
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        Velocity += Acceleration * delta;
        Velocity = new Vector2(Velocity.x * (float)Math.Pow(XDamp, delta), Velocity.y);
        Velocity = MoveAndSlide(Velocity, Vector2.Up);
    }
}
