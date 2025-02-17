using Godot;
using System;

public partial class EnemyTurnState : BattleState
{
    private Unit currentUnit;

    public EnemyTurnState(BattleManager manager, BattleStateMachine machine, Unit unit) : base(manager, machine)
    {
        currentUnit = unit;
    }

    public override void Enter()
    {
        GD.Print($"{currentUnit.UnitName}'s turn (Enemy AI)!");
        EnemyAIController.TakeTurn(currentUnit);
        battleManager.NextTurn();
    }

    public override void Exit() { }
}
