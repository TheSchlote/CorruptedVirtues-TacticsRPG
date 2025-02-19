using Godot;
using System;
using System.Threading.Tasks;

public partial class PlayerTurnState : BattleState
{
    private Unit currentUnit;
    private SelectionCursor cursor;
    private AstarPathfinding astar;
    private GridMap gridMap;

    public PlayerTurnState(BattleManager manager, BattleStateMachine machine, Unit unit) : base(manager, machine)
    {
        currentUnit = unit;
    }

    public override void Enter()
    {
        GD.Print($"{currentUnit.Stats.UnitName}'s turn (Player)!");

        // Retrieve references
        cursor = battleManager.GetNode<SelectionCursor>("SelectionCursor");
        astar = battleManager.GetNode<AstarPathfinding>("PathFinding");
        gridMap = battleManager.GetNode<GridMap>("PathFinding/Map");

        // Move cursor to the current unit's position
        if (cursor != null && currentUnit != null)
        {
            cursor.Position = currentUnit.Position;
            GD.Print($"Cursor moved to {currentUnit.Stats.UnitName} at {cursor.Position}");
        }
        else
        {
            GD.PrintErr("Cursor or current unit is missing. Cannot move cursor.");
        }

        // Enable Player Input Handling
        battleManager.EnablePlayerInput(true);
    }


    public override void Exit()
    {
        // Disable Player Input Handling
        battleManager.EnablePlayerInput(false);
    }


    public void HandlePlayerInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.Enter || keyEvent.Keycode == Key.Space)
            {
                MoveUnitToCursor();
            }
        }
    }

    private async void MoveUnitToCursor()
    {
        if (currentUnit == null || cursor == null || astar == null || gridMap == null)
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
        await currentUnit.MoveTo(targetPosition, astar);

        GD.Print($"{currentUnit.Stats.UnitName} moved to {targetPosition}.");

        // End turn after moving
        battleManager.NextTurn();
    }
}
