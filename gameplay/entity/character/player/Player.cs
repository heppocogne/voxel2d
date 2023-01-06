using Godot;
using Godot.Collections;
using System;
using System.Net.Mime;

public class Player : Character
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
    [Export]
    public float ItemAttractionRange = 16f;
    [Export]
    public float ItemCollectionRange = 8f;

    public bool CursorVisible;
    Coordinate coordinate;
    World worldRoot;
    Vector2 targetCell;
    bool targetCellValid;
    bool leftClick = false;
    bool rightClick = false;
    ButtonList lastClicked;
    DiggingTile digging;
    Timer blockPlaceCooldown;
    int hotbarSlot;
    Inventory inventory;

    // physics
    float jumpInitialVelocity;

    public override void _Ready()
    {
        base._Ready();
        jumpInitialVelocity = (float)Math.Sqrt(Gravity * JumpHeight);
        worldRoot = GetParent<World>();
        coordinate = worldRoot.GetNode<Coordinate>("Coordinate");
        blockPlaceCooldown = GetNode<Timer>("TilePlaceCooldownTimer");
        inventory = GetNode<Inventory>("Inventory");
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

        if (IsOnFloor() && Input.IsActionPressed("game_jump"))
        {
            Velocity = new Vector2(Velocity.x, -jumpInitialVelocity);
        }

        Velocity = MoveAndSlide(Velocity, Vector2.Up);


        if (IsLeftClick() && targetCellValid)
        {
            if (digging == null)
            {
                int id = worldRoot.GetCellv(targetCell);
                if (0 <= id)
                {
                    Dictionary tiledata = worldRoot.GetTileData(id);
                    float hardness = (float)tiledata["Hardness"];
                    if (0 <= hardness)
                    {
                        digging = GD.Load<PackedScene>("res://gameplay/world/tile/digging_tile.tscn").Instance() as DiggingTile;
                        digging.TilePosition = targetCell;
                        digging.Hardness = hardness;
                        digging.Tool = "";
                        Connect(nameof(DigCanceled), digging, "OnCanceled");
                        digging.Connect("TileDestroyed", this, nameof(OnTileDestroyed));
                        digging.Connect("TileDestroyed", worldRoot, nameof(OnTileDestroyed));
                        worldRoot.AddChild(digging);
                        digging.Position = coordinate.MapToWorld(targetCell);
                    }
                }
            }
            else
            {
                digging.Damage(DigDamage * delta);
            }
        }
        else if (!IsLeftClick() && targetCellValid)
        {
            EmitSignal(nameof(DigCanceled));
            digging = null;
        }

        if (IsRightClick() && targetCellValid && worldRoot.GetCellv(targetCell) == -1 && blockPlaceCooldown.TimeLeft == 0.0)
        {
            if (inventory.Items[hotbarSlot] != null)
            {
                worldRoot.SetCellv(targetCell, worldRoot.FindTileID(inventory.Items[hotbarSlot].ItemName));
                inventory.Items[hotbarSlot].Quantity -= 1;
                if (inventory.Items[hotbarSlot].Quantity == 0)
                {
                    inventory.Items[hotbarSlot].QueueFree();
                    inventory.Items[hotbarSlot] = null;
                }
                inventory.InformStateChanged();
                blockPlaceCooldown.Start();
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        //base._Input(@event);
        if (@event is InputEventMouseButton)
        {
            var mb = @event as InputEventMouseButton;
            switch ((ButtonList)mb.ButtonIndex)
            {
                case ButtonList.Left:
                    leftClick = mb.Pressed;
                    lastClicked = ButtonList.Left;
                    break;
                case ButtonList.Right:
                    rightClick = mb.Pressed;
                    lastClicked = ButtonList.Right;
                    blockPlaceCooldown.Stop();
                    break;
            }
        }
    }

    bool IsLeftClick()
    {
        if (leftClick && !rightClick)
            return true;
        else if (leftClick && rightClick)
            return lastClicked == ButtonList.Left;

        return false;
    }

    bool IsRightClick()
    {
        if (rightClick && !leftClick)
            return true;
        else if (leftClick && rightClick)
            return lastClicked == ButtonList.Right;

        return false;
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

    public void OnTileDestroyed(Vector2 pos, String tool)
    {
        digging = null;
        targetCellValid = false;
    }

    public void OnItemAttractionAreaBodyEntered(Node body)
    {
        if (body is Item)
        {
            Item item = body as Item;
            item.Acceleration = 1.1f * Gravity * (Position - item.Position).Normalized();
        }
    }

    public void OnItemAttractionAreaBodyExited(Node body)
    {
        if (body is Item)
        {
            Item item = body as Item;
            if (0 <= inventory.FindAvailableSlot(item))
                item.Acceleration = Vector2.Zero;
        }
    }

    public void OnItemCollectionAreaBodyEntered(Node body)
    {
        if (body is Item)
        {
            Item item = body as Item;
            inventory.CollectItem(item);
        }
    }

    public void OnHotbarStateChanged(int slot)
    {
        hotbarSlot = slot;
    }
}
