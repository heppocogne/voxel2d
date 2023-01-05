using Godot;
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
}
