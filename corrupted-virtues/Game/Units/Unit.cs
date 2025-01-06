using Godot;
using System.Threading.Tasks;

public partial class Unit : Node3D
{
    private const float MOVE_SPEED = 0.5f;  // Time to move between tiles

    public async Task MoveAlongPath(Vector3[] path)
    {
        foreach (Vector3 point in path)
        {
            await MoveTo(point);
        }
    }

    public async Task MoveTo(Vector3 destination)
    {
        float elapsed = 0f;
        float moveTime = MOVE_SPEED * 1.5f;
        Vector3 startPosition = Position;
        Vector3 targetPosition = destination + new Vector3(0, 2, 0);  // Always move to +2 on Y

        while (elapsed < moveTime)
        {
            float t = elapsed / moveTime;
            Position = startPosition.Lerp(targetPosition, t);
            elapsed += 0.025f;
            await Task.Delay(25);
        }

        Position = targetPosition;  // Finalize exact top of the cube
    }

}
