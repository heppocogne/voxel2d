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

    Inventory()
    {
        Items = new Item[Size];
    }

    public override void _Ready()
    {
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

    bool IsCraftable(Dictionary recipe)
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

    bool IsCraftableInternal(Dictionary input)
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

    public Dictionary<Item, Array<Item>> GetAvailableRecipes()
    {
        Dictionary<Item, Array<Item>> result = new Dictionary<Item, Array<Item>>();
        Dictionary recipes = (Dictionary)worldRoot.Recipedata;
        foreach (String outputName in recipes.Keys)
        {
            Dictionary recipe = (Dictionary)recipes[outputName];
            if (recipe["input"] is Array)
            {
                Array inputRecipeVariation = (Array)recipe["input"];
                foreach (Dictionary irv in inputRecipeVariation)
                {
                    if (IsCraftableInternal(irv))
                    {
                        Item output = worldRoot.CreateItemInstance(outputName, (int)(float)recipe["output"]);
                        Array<Item> inputs = new Array<Item>();
                        foreach (String requiredItem in irv.Keys)
                        {
                            Item input = worldRoot.CreateItemInstance(requiredItem, (int)(float)irv[requiredItem]);
                            inputs.Add(input);
                        }
                        result.Add(output, inputs);
                    }
                }
            }
            else if (recipe["input"] is Dictionary)
            {
                if (IsCraftableInternal((Dictionary)recipe["input"]))
                {
                    Item output = worldRoot.CreateItemInstance(outputName, (int)(float)recipe["output"]);
                    Array<Item> inputs = new Array<Item>();
                    var inputItemNames = ((Dictionary)recipe["input"]).Keys;
                    foreach (String requiredItem in inputItemNames)
                    {
                        Item input = worldRoot.CreateItemInstance(requiredItem, (int)(float)((Dictionary)recipe["input"])[requiredItem]);
                        inputs.Add(input);
                    }
                    result.Add(output, inputs);
                }
            }
            else
                GD.PushError("Unexpected recipie format");
        }

        return result;
    }

    void IncreaseItem(Dictionary itemdata)
    {
        String itemname = (String)itemdata["Name"];
        int maxStack = (int)(float)itemdata["Stack"];

        for (int i = 0; i < Items.Length; i++)
        {
            if (Items[i] != null && Items[i].ItemName == itemname && Items[i].Quantity < maxStack)
            {
                Items[i].Quantity++;
                return;
            }
        }

        for (int i = 0; i < Items.Length; i++)
        {
            if (Items[i] == null)
            {
                Items[i] = worldRoot.CreateItemInstance(itemname);
                return;
            }
        }
    }

    void DecreaseItem(Dictionary itemdata)
    {
        String itemname = (String)itemdata["Name"];
        for (int i = 0; i < Items.Length; i++)
        {
            if (Items[i] != null && Items[i].ItemName == itemname)
            {
                Items[i].Quantity--;
                if (Items[i].Quantity == 0)
                {
                    Items[i].QueueFree();
                    Items[i] = null;
                }
                return;
            }
        }
    }

    public void OnCrafted(Item result, Array<Item> consumed)
    {
        Dictionary itemdata = worldRoot.FindItemData(result.ItemName);
        for (int i = 0; i < result.Quantity; i++)
        {
            IncreaseItem(itemdata);
        }
        result.QueueFree();

        for (int i = 0; i < consumed.Count; i++)
        {
            itemdata = worldRoot.FindItemData(consumed[i].ItemName);
            for (int j = 0; j < consumed[i].Quantity; j++)
            {
                DecreaseItem(itemdata);
            }
            consumed[i].QueueFree();
        }

        InformInventoryStateChanged();
    }

    public Dictionary Serialize()
    {
        Dictionary data = new Dictionary();
        Array items = new Array();

        foreach (Item i in Items)
        {
            if (i != null)
                items.Add(i.Serialize());
            else
                items.Add(null);
        }
        data["items"] = items;
        return data;
    }

    public static Inventory Deserialize(Dictionary dic, World world)
    {
        Inventory result = new Inventory();

        Array a = (Array)dic["items"];
        result.Size = a.Count;
        result.Items = new Item[result.Size];
        for (int i = 0; i < result.Size; i++)
        {
            if (a[i] == null)
                result.Items[i] = null;
            else
            {
                result.Items[i] = Item.Deserialize((Dictionary)a[i], world) as Item;
                world.RemoveChild(result.Items[i]);
            }
        }

        return result;
    }
}
