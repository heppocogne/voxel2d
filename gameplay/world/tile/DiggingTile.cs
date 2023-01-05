using Godot;
using System;
using System.Drawing;

public class DiggingTile : Node2D
{
    [Signal]
    delegate void TileDestroyed(Vector2 pos);

    public Vector2 TilePosition;
    public float Hardness;
    public String Tool;
    TextureProgress progress;

    public override void _Ready()
    {
        progress = GetNode<TextureProgress>("TextureProgress");
        progress.MaxValue = Hardness;
        progress.Value = Hardness;
    }

    public void Damage(float damage)
    {
        var prev = progress.Value;
        progress.Value = progress.Value - damage;
        if (progress.Value <= 0)
        {
            EmitSignal(nameof(TileDestroyed), TilePosition, Tool);
            QueueFree();
        }
    }

    public void OnCanceled()
    {
        QueueFree();
    }
}
