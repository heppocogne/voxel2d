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
        foreach (String itemName in worldRoot.Recipedata.Keys)
        {
            GD.Print(itemName, ":", IsCraftable((Dictionary)worldRoot.Recipedata[itemName]));
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
        Dictionary<Godot.Collections.Array, int> required = new Dictionary<Godot.Collections.Array, int>();
        foreach (Dictionary r in (Array)recipe["input"])
        {
            if (r["item"] is Array)
                required.Add((Array)r["item"], (int)(float)r["quantity"]);
            else
                required.Add(new Array { r["item"] }, (int)(float)r["quantity"]);
        }


        foreach (Item item in Items)
        {
            foreach (var pair in required)
            {
                foreach (String n in pair.Key)
                {
                    if (item != null && n == item.ItemName)
                    {
                        required[pair.Key] -= item.Quantity;
                        break;
                    }
                }
            }
        }
        GD.Print(required);

        foreach (int r in required.Values)
        {
            if (0 < r)
                return false;
        }
        return true;
    }
}