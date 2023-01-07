using Godot;
using Godot.Collections;
using System;
using Array = Godot.Collections.Array;

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
                Dictionary d = worldRoot.FindItemData(item.ItemName);
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

        if (item.IsInGroup("Chunk:" + item.ChunkPosition))
            item.RemoveFromGroup("Chunk:" + item.ChunkPosition);

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
        InformInventoryStateChanged();
    }

    public void InformInventoryStateChanged()
    {
        foreach (String item in worldRoot.Recipedata.Keys)
        {
            GD.Print(item, "=", IsCraftable((Dictionary)worldRoot.Recipedata[item]));
        }
        GD.Print("--------");
        EmitSignal(nameof(StateChanged), new Godot.Collections.Array(Items));
    }

    public void OnInventoryManipulated(Array items)
    {
        for (int i = 0; i < Size; i++)
        {
            Items[i] = items[i] as Item;
        }
        InformInventoryStateChanged();
    }

    public bool IsCraftable(Dictionary recipe)
    {
        if (recipe["input"] is Array)
        {
            foreach (Dictionary r in (Array)recipe["input"])
            {
                if (IsCraftableInternal(r))
                    return true;
            }
            return false;
        }
        else if (recipe["input"] is Dictionary)
        {
            return IsCraftableInternal((Dictionary)recipe["input"]);
        }
        GD.PushError("Unexpected recipie format");
        return false;
    }

    private bool IsCraftableInternal(Dictionary input)
    {
        foreach (String key in input.Keys)
        {
            int count = 0;
            int required = (int)(float)input[key];
            foreach (Item item in Items)
            {
                if (item != null && item.ItemName == key)
                {
                    count += item.Quantity;
                    if (required <= count)
                        break;
                }
            }
            if (count < required)
                return false;
        }
        return true;
    }
}