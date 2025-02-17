using Godot;
using System;

public partial class PlayerTurnState : BattleState
{
    private Unit currentUnit;

    public PlayerTurnState(BattleManager manager, BattleStateMachine machine, Unit unit) : base(manager, machine)
    {
        currentUnit = unit;
    }

    public override void Enter()
    {
        GD.Print($"{currentUnit.UnitName}'s turn (Player)!");
        // Enable UI, highlight movement, etc.
    }

    public override void Exit()
    {
        // Disable UI, clear highlights, etc.
    }
}
