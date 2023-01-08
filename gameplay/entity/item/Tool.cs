using Godot;
using Godot.Collections;
using System;

public class Tool : Item
{
    public int MaxDurability = 1;
    public int Durability = 1;

    public override void _Ready()
    {

    }

    public override Dictionary Serialize()
    {
        Dictionary data = base.Serialize();
        data["instance"] = "res://gameplay/entity/item/tool.tscn";
        data["durability"] = Durability;

        return data;
    }
}
