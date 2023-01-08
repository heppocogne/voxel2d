using Godot;
using System;

public class Coordinate : TileMap
{
    [Signal]
    delegate void CellSelected(Vector2 target, bool valid);

    //public Vector2 TargetCell;
    //public bool TargetCellValid;

    World world;
    Player player;
    public override void _Ready()
    {
        world = GetParent<World>();
        player = GetNode<Player>("../Player");
        Connect(nameof(CellSelected), player, "OnCellSelected");
    }

    public override void _Draw()
    {
        //base._Draw();
        if (!player.CursorVisible)
        {
            return;
        }

        Vector2 mapPos = WorldToMap(GetLocalMousePosition());
        int x = (int)mapPos.x;
        int y = (int)mapPos.y;
        int center = world.GetCell(x, y);
        int left = world.GetCell(x - 1, y);
        int right = world.GetCell(x + 1, y);
        int top = world.GetCell(x, y - 1);
        int bottom = world.GetCell(x, y + 1);

        if ((center != -1 || left != -1 || right != -1 || top != -1 || bottom != -1) && (center == -1 || left == -1 || right == -1 || top == -1 || bottom == -1))
        {
            Vector2 lt = mapPos * Chunk.CellSize;
            DrawRect(new Rect2(lt, Chunk.CellSize), new Color(0, 0, 0), false);
            EmitSignal(nameof(CellSelected), mapPos, true);
        }
        else
            EmitSignal(nameof(CellSelected), Vector2.Zero, false);
    }

    public override void _Process(float delta)
    {
        Update();
    }

    static public int MapToChunk(int x)
    {
        if (0 <= x || x % Chunk.ChunkSize == 0)
            return x / Chunk.ChunkSize;
        else
            return x / Chunk.ChunkSize - 1;
    }
}
