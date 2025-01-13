using Godot;
using System.Threading.Tasks;

public partial class Unit : Node3D
{
    private const float CELLS_PER_SECOND = 4f;
    private const float CELL_SIZE = 2f;

    public async Task MoveTo(Vector3 destination, AstarPathfinding pathfinding)
    {
        Vector3[] path = pathfinding.GetPath(Position - new Vector3(0, CELL_SIZE, 0), destination - new Vector3(0, CELL_SIZE, 0));

        if (path.Length == 0)
        {
            GD.Print("Invalid path.");
            return;
        }

        await Move(path);
    }

    public async Task Move(Vector3[] path)
    {
        foreach (Vector3 targetPosition in path)
        {
            Vector3 adjustedTargetPosition = targetPosition + new Vector3(0, CELL_SIZE, 0); // Keep above floor
            Vector3 startPosition = Position;
            float distance = startPosition.DistanceTo(adjustedTargetPosition);
            float moveTime = distance / (CELLS_PER_SECOND * CELL_SIZE);
            float elapsed = 0f;

            while (elapsed < moveTime)
            {
                float t = elapsed / moveTime;
                Position = startPosition.Lerp(adjustedTargetPosition, t);
                elapsed += 0.025f;
                await Task.Delay(25);
            }

            Position = adjustedTargetPosition;
        }
    }
}
