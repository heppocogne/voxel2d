using Godot;
using System;

public class Item : Entity
{
    public String ItemName;
    public int Quantity = 1;

    public override void _Ready()
    {
        base._Ready();
    }
}
