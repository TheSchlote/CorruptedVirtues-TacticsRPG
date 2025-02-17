using Godot;
using System;

public partial class NextTurnState : BattleState
{
    public NextTurnState(BattleManager manager, BattleStateMachine machine) : base(manager, machine) { }

    public override void Enter()
    {
        Unit nextUnit = battleManager.turnQueue.GetNextUnit();
        if (nextUnit == null)
        {
            stateMachine.ChangeState(new EndBattleState(battleManager, stateMachine));
            return;
        }

        if (nextUnit.Team.IsPlayerControlled)
            stateMachine.ChangeState(new PlayerTurnState(battleManager, stateMachine, nextUnit));
        else
            stateMachine.ChangeState(new EnemyTurnState(battleManager, stateMachine, nextUnit));
    }

    public override void Exit() { }
}
