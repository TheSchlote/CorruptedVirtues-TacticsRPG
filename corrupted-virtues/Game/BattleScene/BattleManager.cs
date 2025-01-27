using Godot;
using System;

public partial class BattleManager : Node3D
{
    [Export] private PackedScene UnitScene;
    [Export] public PackedScene BattleMap;

    private GridMap gridMap;
    private AstarPathfinding astar;
    public override void _Ready()
    {
        astar = GetNode<AstarPathfinding>("PathFinding");

        gridMap = BattleMap.Instantiate<GridMap>();
        gridMap.Name = "Map";
        astar.AddChild(gridMap);
        astar.SetupGridMap(gridMap);

        ///TEMP
        //Get spawn position from gridmap
        SpawnUnit(gridMap.GetNode<Node3D>("SpawnPoint").Position);
    }
    public Unit SpawnUnit(Vector3 spawnPosition)
    {
        Vector3I gridPosition = astar.LocalToMap(spawnPosition);

        if (!astar.IsWalkableCell(gridPosition, gridMap))
        {
            GD.Print($"Cannot spawn unit at {spawnPosition}. Cell is not walkable or is occupied.");
            return null;
        }

        var unit = UnitScene.Instantiate<Unit>();
        unit.Position = spawnPosition;
        unit.Name = "TestUnit";
        AddChild(unit);

        astar.MarkCellAsOccupied(gridPosition);

        GD.Print($"Unit spawned at {spawnPosition}.");
        return unit;
    }
}
