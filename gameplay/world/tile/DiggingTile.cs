using Godot;
using System;
using Godot.Collections;

public class DiggingTile : Node2D
{
    [Signal]
    delegate void TileDestroyed(Vector2 pos, bool ToolFitness);

    public Vector2 TilePosition;
    public float Hardness;
    public String ToolMaterial;
    public String ToolKind;
    TextureProgress progress;
    public Dictionary Tiledata;
    public bool ToolFitness;

    public override void _Ready()
    {
        progress = GetNode<TextureProgress>("TextureProgress");
        progress.MaxValue = Hardness;
        progress.Value = Hardness;
    }

    public void CheckToolFitness()
    {
        String tool = (String)Tiledata["Tool"];
        if (tool.Contains(ToolKind))
        {
            if (tool.Contains("diamond"))
            {
                ToolFitness = (ToolMaterial == "diamond");
            }
            else if (tool.Contains("iron"))
            {
                ToolFitness = (ToolMaterial == "diamond") || (ToolMaterial == "iron");
            }
            else if (tool.Contains("stone"))
            {
                ToolFitness = (ToolMaterial == "diamond") || (ToolMaterial == "iron") || (ToolMaterial == "stone") || (ToolMaterial == "golden");
            }
            else if (tool.Contains("wooden"))
            {
                ToolFitness = (ToolMaterial == "diamond") || (ToolMaterial == "iron") || (ToolMaterial == "stone") || (ToolMaterial == "golden") || (ToolMaterial == "wooden");
            }
        }
        else
            ToolFitness = false;
    }

    public void Damage(float damage)
    {
        var prev = progress.Value;
        progress.Value = progress.Value - damage;
        if (progress.Value <= 0)
        {
            EmitSignal(nameof(TileDestroyed), TilePosition, ToolFitness);
            QueueFree();
        }
    }

    public void OnCanceled()
    {
        QueueFree();
    }
}
