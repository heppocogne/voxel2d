using Godot;
using Godot.Collections;
using System;
using System.Linq;
using Array = Godot.Collections.Array;

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

    public bool BlockUserInput = false;
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
    public Inventory Inventory;

    // physics
    float jumpInitialVelocity;

    public override void _Ready()
    {
        base._Ready();
        jumpInitialVelocity = (float)Math.Sqrt(Gravity * JumpHeight);
        worldRoot = GetParent<World>();
        coordinate = worldRoot.GetNode<Coordinate>("Coordinate");
        blockPlaceCooldown = GetNode<Timer>("TilePlaceCooldownTimer");
        Inventory = GetNode<Inventory>("Inventory");
    }

    public override void _Process(float delta)
    {
        CursorVisible = GetLocalMousePosition().Length() < HandLength;
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        if (BlockUserInput)
            return;

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
                        digging.Tiledata = tiledata;
                        if (Inventory.Items[hotbarSlot] is Tool)
                        {
                            String toolName = Inventory.Items[hotbarSlot].ItemName;
                            digging.ToolMaterial = toolName.Left(toolName.Find("_"));
                            digging.ToolKind = toolName.Right(toolName.Find("_") + 1);
                            digging.CheckToolFitness();
                        }
                        else
                        {
                            digging.ToolMaterial = "";
                            digging.ToolKind = "";
                        }
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
                if (digging.ToolMaterial == "" || !digging.ToolFitness)
                    digging.Damage(DigDamage * delta);
                else
                {
                    digging.Damage(DigDamage * delta * (float)worldRoot.FindToolMaterial(digging.ToolMaterial)["Power"]);
                }
            }
        }
        else if (!IsLeftClick() && targetCellValid)
        {
            EmitSignal(nameof(DigCanceled));
            digging = null;
        }

        if (IsRightClick() && targetCellValid && worldRoot.GetCellv(targetCell) == -1 && blockPlaceCooldown.TimeLeft == 0.0)
        {
            if (Inventory.Items[hotbarSlot] != null)
            {
                Dictionary itemdata = worldRoot.FindItemData(Inventory.Items[hotbarSlot].ItemName);
                if (itemdata.Contains("Kind") && (String)itemdata["Kind"] == "tile")
                {
                    worldRoot.SetCellv(targetCell, worldRoot.FindTileID(Inventory.Items[hotbarSlot].ItemName));
                    Inventory.Items[hotbarSlot].Quantity -= 1;
                    if (Inventory.Items[hotbarSlot].Quantity == 0)
                    {
                        Inventory.Items[hotbarSlot].QueueFree();
                        Inventory.Items[hotbarSlot] = null;
                    }
                    Inventory.InformInventoryStateChanged();
                    blockPlaceCooldown.Start();
                }
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

    public void OnTileDestroyed(Vector2 pos, bool ToolFitness)
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
            item.XDamp = 1;
        }
    }

    public void OnItemAttractionAreaBodyExited(Node body)
    {
        if (body is Item)
        {
            Item item = body as Item;
            if (0 <= Inventory.FindAvailableSlot(item))
            {
                item.Acceleration = Vector2.Zero;
                item.XDamp = 0.1f;
            }
        }
    }

    public void OnItemCollectionAreaBodyEntered(Node body)
    {
        if (body is Item)
        {
            Item item = body as Item;
            Inventory.CollectItem(item);
        }
    }

    public void OnHotbarStateChanged(int slot)
    {
        hotbarSlot = slot;
    }

    public void OnPlayerTreeExiting()
    {
        foreach (Item item in Inventory.Items)
        {
            if (item != null)
                item.QueueFree();
        }
    }
}
