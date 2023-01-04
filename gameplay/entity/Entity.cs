using Godot;
using System;

public class Entity : KinematicBody2D
{
    [Signal]
    delegate void ChunkChanged(int newChunk, int oldChyunk);

    [Export]
    public float MaxFallSpeed = 384;

    // state
    public int ChunkPosition = 0;
    public Vector2 Velocity = Vector2.Zero;
    int previousChunk = Int32.MinValue;

    // physics
    protected int gravity;
    protected float airDiag;
    protected Vector2 gravityVector;

    // graphics
    protected bool facingRight = true;   // true=right, false=left
    protected AnimatedSprite sprite;

    public override void _Ready()
    {
        gravity = (int)ProjectSettings.GetSetting("physics/2d/default_gravity");
        gravityVector = (Vector2)ProjectSettings.GetSetting("physics/2d/default_gravity_vector");
        airDiag = gravity / (MaxFallSpeed * MaxFallSpeed);
        sprite = GetNode<AnimatedSprite>("AnimatedSprite");
    }

    public override void _PhysicsProcess(float delta)
    {
        Velocity += (gravity - airDiag * Velocity.y * Velocity.y) * delta * gravityVector;
        Velocity = MoveAndSlide(Velocity, Vector2.Up);

        int currentChunk = World.MapToChunk((int)(Position.x / Chunk.CellSize.x));
        if (currentChunk != previousChunk)
        {
            GD.Print(currentChunk);
            EmitSignal(nameof(ChunkChanged), currentChunk, previousChunk);
            previousChunk = currentChunk;
        }
        ChunkPosition = currentChunk;
    }
}
