using Godot;
using System;

public partial class BattleManager : Node3D
{
    [Export] private PackedScene UnitScene;
    [Export] public PackedScene BattleMap;
    [Export] public PackedScene SelectionCursor;

    private GridMap gridMap;
    private AstarPathfinding astar;
    private SelectionCursor cursor;
    private Unit unit;

    private BattleStateMachine stateMachine;
    public TurnQueue turnQueue;

    public override void _Ready()
    {
        stateMachine = GetNode<BattleStateMachine>("BattleStateMachine");
        turnQueue = new TurnQueue();

        astar = GetNode<AstarPathfinding>("PathFinding");

        cursor = SelectionCursor.Instantiate<SelectionCursor>();
        AddChild(cursor);

        gridMap = BattleMap.Instantiate<GridMap>();
        gridMap.Name = "Map";
        astar.AddChild(gridMap);
        astar.SetupGridMap(gridMap);

        // TEMP: Spawn unit at predefined spawn point
        Vector3 spawnPosition = gridMap.GetNode<Node3D>("SpawnPoint").Position;
        unit = SpawnUnit(spawnPosition);

        stateMachine.Initialize(this);
        stateMachine.ChangeState(new StartState(this, stateMachine));
    }

    public Unit SpawnUnit(Vector3 spawnPosition)
    {
        Vector3I gridPosition = astar.LocalToMap(spawnPosition);

        if (!astar.IsWalkableCell(gridPosition, gridMap))
        {
            GD.Print($"Cannot spawn unit at {spawnPosition}. Cell is not walkable or is occupied.");
            return null;
        }

        Unit spawnedUnit = UnitScene.Instantiate<Unit>();
        spawnedUnit.Position = spawnPosition;
        spawnedUnit.Name = "TestUnit";
        AddChild(spawnedUnit);

        astar.MarkCellAsOccupied(gridPosition);

        GD.Print($"Unit spawned at {spawnPosition}.");
        return spawnedUnit;
    }

    public override void _Process(double delta)
    {
        // Move unit when pressing "ui_accept" (e.g., Enter or Space)
        if (Input.IsActionJustPressed("ui_accept"))
        {
            MoveUnitToCursor();
        }
    }
    public void NextTurn()
    {
        stateMachine.ChangeState(new NextTurnState(this, stateMachine));
    }
    private async void MoveUnitToCursor()
    {
        if (unit == null || cursor == null || astar == null)
        {
            GD.PrintErr("Unit, Cursor, or Pathfinding not found.");
            return;
        }

        Vector3 targetPosition = cursor.GetSelectedTile();
        Vector3I gridTarget = astar.LocalToMap(targetPosition);

        // Ensure the destination is walkable
        if (!astar.IsWalkableCell(gridTarget, gridMap))
        {
            GD.Print($"Blocked: Cannot move to {gridTarget}.");
            return;
        }

        // Move unit using its MoveTo() function
        await unit.MoveTo(targetPosition, astar);

        GD.Print($"Unit moved to {targetPosition}.");
    }
}
