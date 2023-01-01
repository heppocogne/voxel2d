using Godot;
using System;

public class BaseWorldSettings : Resource
{
    public BaseWorldSettings() : this(true) { }
    public BaseWorldSettings(bool randomize)
    {
        if (randomize)
        {
            Seed = (int)((long)GD.Randi() - (2 << 31));
        }
    }

    [Export]
    public int Seed = 0;
    [Export]
    public TileSet TileSet;
    [Export]
    public int MaxHeight = 256;
    [Export]
    public int MinHeight = 0;
    [Export]
    public bool CeilBedrock = false;
    [Export]
    public bool FloorBedrock = true;
    [Export]
    public int MaxBedrockThickness = 5;
    [Export]
    public int MinBedrockThickness = 1;
    [Export]
    public float NoiseScale = 10;
}
