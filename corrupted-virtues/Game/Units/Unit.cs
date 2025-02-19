using Godot;
using System.Threading.Tasks;

public partial class Unit : Node3D
{
    private const float CELLS_PER_SECOND = 4f;
    private const float CELL_SIZE = 2f;

    [Export] public UnitStats Stats { get; set; } // Assignable in the editor
    private Node3D _visualInstance; // Stores the visual model

    public int Health { get;  set; }
    public float Initiative { get; set; }
    public Team Team { get; set; }
    public bool IsAlive => Health > 0;

    public event System.Action<Unit> OnTurnEnd; // Notifies FSM when turn is done

    public override void _Ready()
    {
        if (Stats == null)
        {
            GD.PrintErr("UnitStats not assigned!");
            return;
        }

        Initialize();
        LoadVisual();
    }

    private void Initialize()
    {
        Health = Stats.MaxHealth;
        Initiative = 100f / Stats.Speed; // Higher speed = acts sooner
    }

    private void LoadVisual()
    {
        if (Stats.UnitVisual != null)
        {
            _visualInstance = Stats.UnitVisual.Instantiate<Node3D>();
            _visualInstance.Position = new Vector3(0, 2, 0); //Appear on top of cubes
            AddChild(_visualInstance);
        }
        else
        {
            GD.PrintErr($"No visual assigned for {Stats.UnitName} in UnitStats.");
        }
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
            GD.Print($"{Stats.UnitName} has been defeated!");
        }
        else
        {
            GD.Print($"{Stats.UnitName} took {damage} damage! Remaining HP: {Health}");
        }
    }

    public void Attack(Unit target)
    {
        if (!target.IsAlive)
        {
            GD.Print($"{target.Stats.UnitName} is already down!");
            return;
        }

        int damage = 10; // Simple static damage for now
        target.TakeDamage(damage);
        GD.Print($"{Stats.UnitName} attacks {target.Stats.UnitName} for {damage} damage!");
    }

    public void IncreaseInitiative()
    {
        Initiative += (100f / Stats.Speed);
        GD.Print($"{Stats.UnitName}'s Initiative increased to {Initiative}");
    }

    public void EndTurn()
    {
        IncreaseInitiative();
        OnTurnEnd?.Invoke(this);
    }
}
