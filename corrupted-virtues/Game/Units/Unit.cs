using Godot;
using System.Threading.Tasks;

public partial class Unit : Node3D
{
    private const float CELLS_PER_SECOND = 4f;
    private const float CELL_SIZE = 2f;

    public string UnitName { get; private set; }
    public int Health { get; private set; }
    public int MaxHealth { get; private set; } = 100;
    public int Speed { get; private set; } // Determines how often a unit acts
    public float Initiative { get; private set; } // Tracks when a unit acts next
    public Team Team { get; private set; }
    public bool IsAlive => Health > 0;

    public event System.Action<Unit> OnTurnEnd; // Notifies FSM when turn is done

    public void Initialize(string name, int speed, Team team)
    {
        UnitName = name;
        Speed = speed;
        Team = team;
        Health = MaxHealth;
        Initiative = 100f / Speed; // Higher speed = acts sooner
    }

    public async Task MoveTo(Vector3 destination, AstarPathfinding pathfinding)
    {
        Vector3I currentCell = pathfinding.LocalToMap(Position);
        pathfinding.MarkCellAsUnoccupied(currentCell);

        Vector3[] path = pathfinding.GetPath(Position, destination);

        if (path.Length == 0)
        {
            GD.Print("Invalid path. Restoring occupancy of current cell.");
            pathfinding.MarkCellAsOccupied(currentCell);
            return;
        }

        await Move(path);

        Vector3I destinationCell = pathfinding.LocalToMap(Position);
        pathfinding.MarkCellAsOccupied(destinationCell);
    }

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

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            Health = 0;
            GD.Print($"{UnitName} has been defeated!");
        }
        else
        {
            GD.Print($"{UnitName} took {damage} damage! Remaining HP: {Health}");
        }
    }

    public void Attack(Unit target)
    {
        if (!target.IsAlive)
        {
            GD.Print($"{target.UnitName} is already down!");
            return;
        }

        int damage = 10; // Simple static damage for now
        target.TakeDamage(damage);
        GD.Print($"{UnitName} attacks {target.UnitName} for {damage} damage!");
    }

    public void IncreaseInitiative()
    {
        Initiative += (100f / Speed); // The faster the unit, the sooner they act again
        GD.Print($"{UnitName}'s Initiative increased to {Initiative}");
    }

    public void EndTurn()
    {
        IncreaseInitiative(); //  Increases Initiative before re-entering the queue
        OnTurnEnd?.Invoke(this); // Notify BattleManager or FSM
    }
}
