using Godot;
using System;

public partial class Unit : Node3D
{
    private const int CELL_SIZE = 2;  // 2x2x2 grid size

    public void MoveTo(Vector3 direction)
    {
        Position += direction * CELL_SIZE;
    }
}