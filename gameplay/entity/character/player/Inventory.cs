using Godot;
using Godot.Collections;
using System;

public class Inventory : Node
{
    [Signal]
    delegate void StateChanged(Array<Item> items);

    [Export]
    public int Size = 36;

    public Item[] Items;

    World worldRoot;

    public override void _Ready()
    {
        Items = new Item[Size];
        worldRoot = GetTree().Root.GetNode<World>("GameScreen/ViewportContainer/Viewport/World");
    }

    public int FindAvailableSlot(Item item)
    {
        for (int i = 0; i < Size; i++)
        {
            if (Items[i] != null && Items[i].ItemName == item.ItemName)
            {
                Dictionary d = worldRoot.GetItemData(item.ItemName);
                int stack = (int)(float)d["Stack"];
                if (stack == -1 || Items[i].Quantity < stack)
                {
                    return i;
                }
            }
        }

        for (int i = 0; i < Size; i++)
        {
            if (Items[i] == null)
            {
                return i;
            }
        }

        return -1;
    }

    public void CollectItem(Item item)
    {
        int slot = FindAvailableSlot(item);
        if (slot == -1)
            return;

        if (Items[slot] == null)
        {
            Items[slot] = item;
            worldRoot.RemoveChild(item);
        }
        else
        {
            Items[slot].Quantity++;
            item.QueueFree();
        }
        InformStateChanged();
    }

    public void InformStateChanged()
    {
        EmitSignal(nameof(StateChanged), new Godot.Collections.Array(Items));
    }
}