using Godot;
using Godot.Collections;
using System;

public class Character : Entity
{
    // graphics
    protected bool facingRight = true;   // true=right, false=left
    protected AnimatedSprite sprite;

    public override void _Ready()
    {
        base._Ready();
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");
    }

    public override Dictionary Serialize()
    {
        Dictionary data = base.Serialize();
        //data["instance"] = "res://gameplay/entity/character/character.tscn";
        data["facing_right"] = facingRight;

        return data;
    }

    protected override Entity _DeserializeImpl(Dictionary dic, World world)
    {
        facingRight = (bool)dic["facing_right"];

        return this;
    }
}
