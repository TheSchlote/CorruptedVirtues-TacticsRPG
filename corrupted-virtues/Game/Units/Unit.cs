using Godot;
using System.Threading.Tasks;

public partial class Unit : Node3D
{
    private const float CELLS_PER_SECOND = 4f;
    private const float CELL_SIZE = 2f;

    public async Task MoveTo(Vector3 destination, AstarPathfinding pathfinding)
    {
        Vector3I currentCell = pathfinding.LocalToMap(Position);
        pathfinding.MarkCellAsUnoccupied(currentCell);

        Vector3[] path = pathfinding.GetPath(Position, destination);

        if (path.Length == 0)
        {
            GD.Print("Invalid path. Restoring occupancy of current cell.");
            pathfinding.MarkCellAsOccupied(currentCell); // Restore the original cell if pathfinding fails
            return;
        }

        await Move(path);

        Vector3I destinationCell = pathfinding.LocalToMap(Position);
        pathfinding.MarkCellAsOccupied(destinationCell);
    }

    //public async Task MoveTo(Vector3 destination, AstarPathfinding pathfinding)
    //{
    //    Vector3[] path = pathfinding.GetPath(Position, destination);

    //    if (path.Length == 0)
    //    {
    //        GD.Print("Invalid path.");
    //        return;
    //    }

    //    await Move(path);
    //}

    public async Task Move(Vector3[] path)
    {
        foreach (Vector3 targetPosition in path)
        {
            Vector3 startPosition = Position;
            float distance = startPosition.DistanceTo(targetPosition);
            float moveTime = distance / (CELLS_PER_SECOND * CELL_SIZE);
            float elapsed = 0f;

            while (elapsed < moveTime)
            {
                float t = elapsed / moveTime;
                Position = startPosition.Lerp(targetPosition, t);
                elapsed += 0.025f;
                await Task.Delay(25);
            }

            Position = targetPosition;
        }
    }
}
