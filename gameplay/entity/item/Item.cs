using Godot;
using Godot.Collections;
using System;

public class Item : Entity
{
    public Texture ItemTexture;
    public String ItemName;
    public int Quantity = 1;

    // physics
    public Vector2 Acceleration;
    public float XDamp;

    public override void _Ready()
    {
        base._Ready();
        GetNode<Sprite>("Sprite").Texture = ItemTexture;
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);

        Velocity += Acceleration * delta;
        Velocity = new Vector2(Velocity.x * (float)Math.Pow(XDamp, delta), Velocity.y);
        Velocity = MoveAndSlide(Velocity, Vector2.Up);
    }

    public override Dictionary Serialize()
    {
        Dictionary data = base.Serialize();
        //data["instance"] = "res://gameplay/entity/item/item.tscn";
        data["item_name"] = ItemName;
        data["quantity"] = Quantity;

        return data;
    }

    protected override Entity _DeserializeImpl(Dictionary dic, World world)
    {
        ItemName = (String)dic["item_name"];
        Quantity = (int)dic["quantity"];
        ItemTexture = GD.Load<Texture>((String)world.FindItemData(ItemName)["Texture"]);
        GetNode<Sprite>("Sprite").Texture = ItemTexture;

        return this;
    }
}
