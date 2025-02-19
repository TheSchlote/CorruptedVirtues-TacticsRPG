using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class BattleManager : Node3D
{
    [Export] private PackedScene UnitScene;
    [Export] public PackedScene BattleMap;
    [Export] public PackedScene SelectionCursor;
    [Export] public Godot.Collections.Array<UnitStats> PlayerUnits = new();
    [Export] public Godot.Collections.Array<UnitStats> EnemyUnits = new();

    private List<Team> teams = new(); // Stores all teams

    private GridMap gridMap;
    private AstarPathfinding astar;
    private SelectionCursor cursor;

    private BattleStateMachine stateMachine;
    public TurnQueue turnQueue;
    private bool isPlayerInputEnabled = false;

    public void EnablePlayerInput(bool enable)
    {
        isPlayerInputEnabled = enable;
    }

    public override void _Ready()
    {
        stateMachine = GetNode<BattleStateMachine>("BattleStateMachine");
        turnQueue = new TurnQueue();
        astar = GetNode<AstarPathfinding>("PathFinding");

        InitializeCursor();
        InitializeBattleMap();

        // Create Teams (each gets a unique ID)
        Team playerTeam = new Team(1, true);
        Team enemyTeam = new Team(2, false);

        teams.Add(playerTeam);
        teams.Add(enemyTeam);

        // Get all spawn points
        List<Node3D> allSpawnPoints = new List<Node3D>();
        foreach (Node node in gridMap.GetTree().GetNodesInGroup("SpawnPoint"))
        {
            if (node is Node3D spawnPoint)
            {
                allSpawnPoints.Add(spawnPoint);
            }
        }

        // Sort spawn points into Player and Enemy groups
        List<Node3D> playerSpawnPoints = allSpawnPoints.Where(spawn => spawn.Name.ToString().Contains("Player")).ToList();
        List<Node3D> enemySpawnPoints = allSpawnPoints.Where(spawn => spawn.Name.ToString().Contains("Enemy")).ToList();

        // Ensure enough spawn points exist
        if (playerSpawnPoints.Count < PlayerUnits.Count)
        {
            GD.PrintErr($"Not enough player spawn points! Found {playerSpawnPoints.Count}, need {PlayerUnits.Count}.");
            return;
        }
        if (enemySpawnPoints.Count < EnemyUnits.Count)
        {
            GD.PrintErr($"Not enough enemy spawn points! Found {enemySpawnPoints.Count}, need {EnemyUnits.Count}.");
            return;
        }

        // Assign spawn points for Player units (manual placement in the future)
        SpawnUnitsForTeam(playerTeam, PlayerUnits, playerSpawnPoints);

        // Assign spawn points for Enemy units (auto-fill for now)
        SpawnUnitsForTeam(enemyTeam, EnemyUnits, enemySpawnPoints);

        // Initialize Turn Queue
        turnQueue.InitializeQueue(teams);

        StartBattle();
    }

    private void StartBattle()
    {
        stateMachine.Initialize(this);
        stateMachine.ChangeState(new StartState(this, stateMachine));
    }

    private void InitializeBattleMap()
    {
        gridMap = BattleMap.Instantiate<GridMap>();
        gridMap.Name = "Map";
        astar.AddChild(gridMap);
        astar.SetupGridMap(gridMap);
    }

    private void InitializeCursor()
    {
        cursor = SelectionCursor.Instantiate<SelectionCursor>();
        AddChild(cursor);
    }

    private void SpawnUnitsForTeam(Team team, Godot.Collections.Array<UnitStats> unitStatsList, List<Node3D> spawnPoints)
    {
        for (int i = 0; i < unitStatsList.Count; i++)
        {
            if (i >= spawnPoints.Count)
            {
                GD.PrintErr($"Not enough spawn points for Team {team.TeamID}!");
                return;
            }

            Node3D spawnPoint = spawnPoints[i];
            Unit unit = SpawnUnit(spawnPoint.Position, unitStatsList[i], team);
            team.AddUnit(unit);
        }
    }

    private Unit SpawnUnit(Vector3 spawnPosition, UnitStats stats, Team team)
    {
        Vector3I gridPosition = astar.LocalToMap(spawnPosition);

        if (!astar.IsWalkableCell(gridPosition, gridMap))
        {
            GD.PrintErr($"Cannot spawn unit at {spawnPosition}. Cell is not walkable or occupied.");
            return null;
        }

        Unit spawnedUnit = UnitScene.Instantiate<Unit>();
        spawnedUnit.Position = spawnPosition;
        spawnedUnit.Stats = stats;
        spawnedUnit.Team = team;
        AddChild(spawnedUnit);

        astar.MarkCellAsOccupied(gridPosition);
        GD.Print($"{stats.UnitName} spawned at {spawnPosition} in Team {team.TeamID}.");

        return spawnedUnit;
    }

    public void NextTurn()
    {
        stateMachine.ChangeState(new NextTurnState(this, stateMachine));
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!isPlayerInputEnabled) return; // Ignore input if disabled

        if (stateMachine.currentState is PlayerTurnState playerTurnState)
        {
            playerTurnState.HandlePlayerInput(@event);
        }
    }
}
