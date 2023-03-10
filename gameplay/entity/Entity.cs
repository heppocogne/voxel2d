using Godot;
using Godot.Collections;
using System;

public abstract class Entity : KinematicBody2D
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
    public int Gravity;
    protected float airDiag;
    protected Vector2 gravityVector;

    public override void _Ready()
    {
        Gravity = (int)ProjectSettings.GetSetting("physics/2d/default_gravity");
        gravityVector = (Vector2)ProjectSettings.GetSetting("physics/2d/default_gravity_vector");
        airDiag = Gravity / (MaxFallSpeed * MaxFallSpeed);
    }

    public override void _PhysicsProcess(float delta)
    {
        Velocity += (Gravity - airDiag * Velocity.y * Velocity.y) * delta * gravityVector;
        Velocity = MoveAndSlide(Velocity, Vector2.Up);

        int currentChunk = Coordinate.MapToChunk((int)(Position.x / Chunk.CellSize.x));
        if (currentChunk != previousChunk)
        {
            String previousGroup = "Chunk:" + GD.Str(previousChunk);
            if (IsInGroup(previousGroup))
                RemoveFromGroup(previousGroup);
            AddToGroup("Chunk:" + GD.Str(currentChunk));
            EmitSignal(nameof(ChunkChanged), currentChunk, previousChunk);
            previousChunk = currentChunk;
        }
        ChunkPosition = currentChunk;
    }

    public virtual Dictionary Serialize()
    {
        Dictionary data = new Dictionary();
        data["instance"] = Filename;
        data["position"] = Position;

        return data;
    }

    protected abstract Entity _DeserializeImpl(Dictionary dic, World world);

    public static Entity Deserialize(Dictionary dic, World world)
    {
        Entity entity = GD.Load<PackedScene>((String)dic["instance"]).Instance<Entity>();
        world.AddChild(entity);
        entity.Position = (Vector2)dic["position"];
        return entity._DeserializeImpl(dic, world);
    }
}
