using Godot;
using System;

public partial class StartState : BattleState
{
    public StartState(BattleManager manager, BattleStateMachine machine) : base(manager, machine) { }

    public override void Enter()
    {
        GD.Print("Battle Started!");
        stateMachine.ChangeState(new NextTurnState(battleManager, stateMachine));
    }

    public override void Exit() { }
}
