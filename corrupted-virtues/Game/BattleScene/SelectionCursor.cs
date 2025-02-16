using Godot;

public partial class SelectionCursor : Node3D
{
    [Export] public float MoveSpeed { get; set; } = 10f;
    [Export] public float SnapDelay { get; set; } = 0.2f;
    [Export] public float SnapSpeed { get; set; } = 0.2f;
    [Export] public float HeightTransitionSpeed { get; set; } = 0.1f;
    [Export] public Gimbal CameraGimbal { get; set; }

    private AstarPathfinding pathfinding;
    private GridMap gridMap;
    private Vector3 cellSize = new Vector3(2, 2, 2); // Default, overridden dynamically
    private Vector3 velocity = Vector3.Zero;
    private Vector3 targetGridPosition;
    private float snapTimer = 0f;
    private bool shouldSnap = false;
    private bool pathfindingFound = false;

    public override void _Process(double delta)
    {
        if (!pathfindingFound)
        {
            pathfinding = GetTree().Root.FindChild("PathFinding", true, false) as AstarPathfinding;
            if (pathfinding != null)
            {
                gridMap = pathfinding.GetNode<GridMap>("Map");
                cellSize = gridMap.CellSize; // Dynamically set the GridMap cell size
                pathfindingFound = true;
                GD.Print("GridMap detected. Using CellSize: " + cellSize);
            }
            else
            {
                GD.PrintErr("Could not find PathFinding. Ensure it is in the scene.");
            }
        }

        HandleInput(delta);
        MoveCursor(delta);

        if (Input.IsActionJustPressed("ui_accept"))
        {
            GD.Print("Cursor is on tile: " + pathfinding.LocalToMap(Position));
        }
    }

    private void HandleInput(double delta)
    {
        if (CameraGimbal == null || pathfinding == null || gridMap == null) return;

        Vector3 inputDirection = GetMovementDirection();
        if (inputDirection == Vector3.Zero)
        {
            velocity = Vector3.Zero;
            HandleSnapTimer(delta);
            return;
        }

        Vector3I targetCell = pathfinding.LocalToMap(Position + inputDirection * cellSize);
        targetCell = GetBestAvailableCell(targetCell);

        if (gridMap.GetCellItem(targetCell) != -1)
        {
            velocity = inputDirection * MoveSpeed;
            shouldSnap = true;
            snapTimer = 0f;
        }
        else
        {
            GD.Print("Blocked: Cannot move to " + targetCell);
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
        if (pathfinding == null || gridMap == null) return;

        Vector3I targetCell = GetBestAvailableCell(pathfinding.LocalToMap(Position));
        float targetY = targetCell.Y * cellSize.Y;

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
        if (pathfinding == null || gridMap == null) return;

        Vector3I closestTile = GetBestAvailableCell(pathfinding.LocalToMap(Position));
        Vector3 snappedPosition = closestTile * cellSize;
        targetGridPosition = snappedPosition;

        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(this, "position", snappedPosition, SnapSpeed)
             .SetTrans(Tween.TransitionType.Sine)
             .SetEase(Tween.EaseType.InOut);
    }

    private Vector3I GetBestAvailableCell(Vector3I cell)
    {
        if (gridMap == null) return cell;

        Vector3I lowerCell = cell + Vector3I.Down;
        Vector3I upperCell = cell + Vector3I.Up;

        if (gridMap.GetCellItem(cell) != -1) return cell;
        if (gridMap.GetCellItem(lowerCell) != -1) return lowerCell;
        if (gridMap.GetCellItem(upperCell) != -1) return upperCell;

        return FindClosestValidTile(gridMap, cell);
    }

    private Vector3I FindClosestValidTile(GridMap gridMap, Vector3I startTile)
    {
        Vector3I[] directions = { Vector3I.Right, Vector3I.Left, Vector3I.Forward, Vector3I.Back, Vector3I.Up, Vector3I.Down };
        int searchRadius = 5;

        for (int radius = 1; radius <= searchRadius; radius++)
        {
            foreach (Vector3I direction in directions)
            {
                Vector3I checkTile = startTile + (direction * radius);
                if (gridMap.GetCellItem(checkTile) != -1)
                {
                    GD.Print("Found closest valid tile at: " + checkTile);
                    return checkTile;
                }
            }
        }

        GD.PrintErr("No valid tile found near " + startTile + "! Cursor may be stuck.");
        return pathfinding.LocalToMap(targetGridPosition);
    }

    public Vector3 GetSelectedTile()
    {
        return targetGridPosition;
    }
}
