using Godot;
using Godot.Collections;
using System;
using System.Net.Mime;

public class Player : Entity
{
    [Signal]
    delegate void DigCanceled();

    [Export]
    public float HandLength = 4.5f * 16;
    [Export]
    public float WalkSpeed = 96;
    [Export]
    public float RunSpeed = 128;
    [Export]
    public float JumpHeight = 20;
    [Export]
    public float DigDamage = 0.2f;

    public bool CursorVisible;
    Coordinate coordinate;
    World worldRoot;
    Vector2 targetCell;
    bool targetCellValid;
    bool leftClick = false;
    DiggingTile digging;

    // physics
    float jumpInitialVelocity;

    public override void _Ready()
    {
        base._Ready();
        jumpInitialVelocity = (float)Math.Sqrt(gravity * JumpHeight);
        worldRoot = GetParent<World>();
        coordinate = worldRoot.GetNode<Coordinate>("Coordinate");
    }

    public override void _Process(float delta)
    {
        CursorVisible = GetLocalMousePosition().Length() < HandLength;
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        bool l = Input.IsActionPressed("game_left");
        bool r = Input.IsActionPressed("game_right");
        if (l && !r)
        {
            MoveAndCollide(new Vector2(-WalkSpeed, 0) * delta);
            sprite.Play("left_walk");
            facingRight = false;
        }
        else if (r && !l)
        {
            MoveAndCollide(new Vector2(WalkSpeed, 0) * delta);
            sprite.Play("right_walk");
            facingRight = true;
        }
        else
        {
            if (facingRight)
            {
                sprite.Play("right_walk");
                sprite.Playing = false;
                sprite.Frame = 1;
            }
            else
            {
                sprite.Play("left_walk");
                sprite.Playing = false;
                sprite.Frame = 1;
            }
        }

        if (IsOnFloor() && Input.IsActionJustPressed("game_jump"))
        {
            Velocity = new Vector2(Velocity.x, -jumpInitialVelocity);
        }

        Velocity = MoveAndSlide(Velocity, Vector2.Up);


        if (leftClick && targetCellValid)
        {
            if (digging == null)
            {
                int id = worldRoot.GetCellv(targetCell);
                if (0 <= id)
                {
                    digging = GD.Load<PackedScene>("res://gameplay/world/tile/digging_tile.tscn").Instance() as DiggingTile;
                    digging.TilePosition = targetCell;
                    Dictionary tiledata = worldRoot.GetTileData(id);
                    digging.Solidity = (float)tiledata["Solidity"];
                    Connect(nameof(DigCanceled), digging, "OnCanceled");
                    digging.Connect("TileDestroyed", this, nameof(OnTileDestroyed));
                    digging.Connect("TileDestroyed", worldRoot, nameof(OnTileDestroyed));
                    worldRoot.AddChild(digging);
                    digging.Position = coordinate.MapToWorld(targetCell);
                }
            }
            else
            {
                digging.Damage(DigDamage * delta);
            }
        }
        else if (!leftClick && targetCellValid)
        {
            EmitSignal(nameof(DigCanceled));
            digging = null;
        }
    }

    public override void _Input(InputEvent @event)
    {
        //base._Input(@event);
        if (@event is InputEventMouseButton)
        {
            var mb = @event as InputEventMouseButton;
            switch (mb.ButtonIndex)
            {
                case (int)ButtonList.Left:
                    leftClick = mb.Pressed;

                    break;
            }
        }
    }

    public void OnCellSelected(Vector2 cell, bool valid)
    {
        if (cell != targetCell && digging != null)
        {
            EmitSignal(nameof(DigCanceled));
            digging = null;
        }
        targetCell = cell;
        targetCellValid = valid;
    }

    public void OnTileDestroyed(Vector2 pos)
    {
        digging = null;
        targetCellValid = false;
    }
}
