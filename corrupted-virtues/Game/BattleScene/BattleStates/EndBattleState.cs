using Godot;
using System;

public partial class EndBattleState : BattleState
{
    public EndBattleState(BattleManager manager, BattleStateMachine machine) : base(manager, machine) { }

    public override void Enter()
    {
        GD.Print("Battle Over!");
        // Show results screen, transition back to overworld, etc.
    }

    public override void Exit() { }
}
