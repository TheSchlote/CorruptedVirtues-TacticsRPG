using Godot;
using System;

public partial class TurnEndState : BattleState
{
    private Unit currentUnit;

    public TurnEndState(BattleManager manager, BattleStateMachine machine, Unit unit) : base(manager, machine)
    {
        currentUnit = unit;
    }

    public override void Enter()
    {
        GD.Print($"Applying end-of-turn effects for {currentUnit.UnitName}");
        battleManager.NextTurn();
    }

    public override void Exit() { }
}
