using Godot;

public static class EnemyAIController
{
    public static void TakeTurn(Unit enemy)
    {
        GD.Print($"{enemy.Name} chooses an action...");
        // Simple AI: Move toward player and attack
        GD.Print($"{enemy.Name} skips turn for now!");
    }
}
