using Godot;
using System;

public class DiggingTile : Node2D
{
    [Signal]
    delegate void TileDestroyed(Vector2 pos);

    public Vector2 TilePosition;
    public float Solidity;
    TextureProgress progress;

    public override void _Ready()
    {
        progress = GetNode<TextureProgress>("TextureProgress");
        progress.MaxValue = Solidity;
        progress.Value = Solidity;
    }

    public void Damage(float damage)
    {
        var prev = progress.Value;
        progress.Value = progress.Value - damage;
        if (progress.Value <= 0)
        {
            EmitSignal(nameof(TileDestroyed), TilePosition);
            QueueFree();
        }
    }

    public void OnCanceled()
    {
        QueueFree();
    }
}
