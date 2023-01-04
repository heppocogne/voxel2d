using Godot;
using System;

public class Coordinate : TileMap
{
    [Signal]
    delegate void CellSelected(Vector2 target);

    //public Vector2 TargetCell;
    //public bool TargetCellValid;

    ChunkManager manager;
    Player player;
    public override void _Ready()
    {
        manager = GetNode<ChunkManager>("../ChunkManager");
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
        int center = manager.GetCell(x, y);
        int left = manager.GetCell(x - 1, y);
        int right = manager.GetCell(x + 1, y);
        int top = manager.GetCell(x, y - 1);
        int bottom = manager.GetCell(x, y + 1);

        if ((center != -1 || left != -1 || right != -1 || top != -1 || bottom != -1) && (center == -1 || left == -1 || right == -1 || top == -1 || bottom == -1))
        {
            Vector2 lt = mapPos * Chunk.CellSize;
            DrawRect(new Rect2(lt, Chunk.CellSize), new Color(0, 0, 0), false);
            EmitSignal(nameof(CellSelected), mapPos);
        }
    }

    public override void _Process(float delta)
    {
        Update();
    }
}
