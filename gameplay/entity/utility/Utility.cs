using Godot;
using Godot.Collections;
using System;

public class Utility : Entity
{
    [Export]
    public PackedScene InventoryScene;
    public Inventory Inventory;

    public override void _Ready()
    {
        Inventory = GetNode<Inventory>("Inventory");
        SetPhysicsProcess(false);
    }

    public void OpenInventory()
    {
        GD.Print(Inventory.Items);
        Control gui = InventoryScene.Instance<Control>();
        gui.Set("utility_node", this);
        GetTree().Root.GetNode("GameScreen/ViewportContainer/GUILayer/GUI").EmitSignal("inventory_opened");
        GetTree().Root.GetNode("GameScreen/ViewportContainer/GUILayer/GUI").Set("inventory", gui);
        GetTree().Root.GetNode("GameScreen/ViewportContainer/GUILayer/GUI/InventoryContainer").AddChild(gui);
    }

    public override Dictionary Serialize()
    {
        Dictionary data = base.Serialize();
        data["inventory"] = Inventory.Serialize();

        return data;
    }

    protected override Entity _DeserializeImpl(Dictionary dic, World world)
    {
        RemoveChild(Inventory);
        Inventory.QueueFree();
        Inventory = Inventory.Deserialize((Dictionary)dic["inventory"], world);
        AddChild(Inventory);
        Inventory.Name = "Inventory";
        Inventory.CallDeferred("InformInventoryStateChanged");

        return this;
    }
}
