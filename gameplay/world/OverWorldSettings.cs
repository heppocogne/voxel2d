using Godot;
using System;

public class OverWorldSettings : BaseWorldSettings
{
    public OverWorldSettings() : base() { }
    public OverWorldSettings(bool randomize) : base(randomize) { }


    [Export]
    public int MaxGroundHeight = 80;
    [Export]
    public int MinGroundHeight = 50;
    [Export]
    public int MaxDirtThickness = 5;
    [Export]
    public int MinDirtThickness = 3;
}
