using Godot;

public partial class SelectionCursor : Node3D
{
    [Export] public Vector3 CellSize { get; set; } = new Vector3(2, 2, 2);
    [Export] public float MoveSpeed { get; set; } = 5f;
    [Export] public float SnapDelay { get; set; } = 0.2f;
    [Export] public float SnapSpeed { get; set; } = 0.2f;
    [Export] public float HeightTransitionSpeed { get; set; } = 0.1f;
    [Export] public Gimbal CameraGimbal { get; set; }

    private AstarPathfinding pathfinding;
    private Vector3 velocity = Vector3.Zero;
    private Vector3 targetGridPosition;
    private float snapTimer = 0f;
    private bool shouldSnap = false;
    private bool pathfindingFound = false;

    public override void _Process(double delta)
    {
        if (!pathfindingFound)
        {
            FindPathfinding();
        }

        HandleInput(delta);
        MoveCursor(delta);
    }

    private void FindPathfinding()
    {
        Node foundNode = GetTree().Root.FindChild("PathFinding", true, false);
        if (foundNode is AstarPathfinding foundPathfinding)
        {
            pathfinding = foundPathfinding;
            GD.Print("Found AstarPathfinding in scene dynamically.");
            pathfindingFound = true;
        }
        else
        {
            GD.PrintErr("Could not find PathFinding. Ensure it is in the scene.");
        }
    }

    private void HandleInput(double delta)
    {
        if (CameraGimbal == null || pathfinding == null)
        {
            return;
        }

        Vector3 inputDirection = GetMovementDirection();

        if (inputDirection != Vector3.Zero)
        {
            velocity = inputDirection * MoveSpeed;
            shouldSnap = true;
            snapTimer = 0f;
        }
        else
        {
            velocity = Vector3.Zero;
            HandleSnapTimer(delta);
        }
    }

    private Vector3 GetMovementDirection()
    {
        Vector3 inputDirection = Vector3.Zero;
        Vector3 forward = CameraGimbal.Transform.Basis.Z.Normalized();
        Vector3 right = CameraGimbal.Transform.Basis.X.Normalized();

        if (Input.IsActionPressed("ui_right")) inputDirection += right;
        if (Input.IsActionPressed("ui_left")) inputDirection -= right;
        if (Input.IsActionPressed("ui_down")) inputDirection += forward;
        if (Input.IsActionPressed("ui_up")) inputDirection -= forward;

        inputDirection.Y = 0;
        return inputDirection.Normalized();
    }

    private void HandleSnapTimer(double delta)
    {
        if (shouldSnap)
        {
            snapTimer += (float)delta;
            if (snapTimer >= SnapDelay)
            {
                SnapToGrid();
                shouldSnap = false;
            }
        }
    }

    private void MoveCursor(double delta)
    {
        if (velocity.Length() > 0.01f)
        {
            Position += velocity * (float)delta;
            UpdateCursorHeight();
        }
    }
    private void UpdateCursorHeight()
    {
        if (pathfinding == null)
        {
            return;
        }

        GridMap gridMap = pathfinding.GetNodeOrNull<GridMap>("Map");
        if (gridMap == null)
        {
            GD.PrintErr("Could not find Map inside PathFinding.");
            return;
        }

        Vector3I targetCell = GetAdjustedHeightCell(gridMap);
        float targetY = targetCell.Y * CellSize.Y;

        if (!Mathf.IsEqualApprox(Position.Y, targetY))
        {
            Tween tween = GetTree().CreateTween();
            tween.TweenProperty(this, "position:y", targetY, HeightTransitionSpeed)
                 .SetTrans(Tween.TransitionType.Sine)
                 .SetEase(Tween.EaseType.InOut);
        }
    }

    private void SnapToGrid()
    {
        if (pathfinding == null)
        {
            return;
        }

        GridMap gridMap = pathfinding.GetNodeOrNull<GridMap>("Map");
        if (gridMap == null)
        {
            GD.PrintErr("Could not find Map inside PathFinding.");
            return;
        }

        Vector3I targetCell = GetAdjustedHeightCell(gridMap);
        Vector3 snappedPosition = targetCell * CellSize;

        // No more IsWalkableCell check, just snap to any valid tile
        if (gridMap.GetCellItem(targetCell) != -1)
        {
            Tween tween = GetTree().CreateTween();
            tween.TweenProperty(this, "position", snappedPosition, SnapSpeed)
                 .SetTrans(Tween.TransitionType.Sine)
                 .SetEase(Tween.EaseType.InOut);

            targetGridPosition = snappedPosition;
        }
        else
        {
            GD.Print("Blocked: Cannot snap to " + targetCell);
        }
    }

    private Vector3I GetAdjustedHeightCell(GridMap gridMap)
    {
        Vector3I currentCell = pathfinding.LocalToMap(Position);
        Vector3I targetCell = currentCell;

        Vector3I lowerCell = targetCell + Vector3I.Down;
        Vector3I upperCell = targetCell + Vector3I.Up;

        // Instead of checking IsWalkableCell, just check if a tile exists at that position
        if (gridMap.GetCellItem(lowerCell) != -1)
        {
            targetCell = lowerCell;
        }
        else if (gridMap.GetCellItem(upperCell) != -1)
        {
            targetCell = upperCell;
        }

        return targetCell;
    }

    public Vector3 GetSelectedTile()
    {
        return targetGridPosition;
    }
}
