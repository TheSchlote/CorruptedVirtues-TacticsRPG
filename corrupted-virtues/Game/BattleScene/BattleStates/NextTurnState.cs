using Godot;
using System;

public partial class NextTurnState : BattleState
{
    public NextTurnState(BattleManager manager, BattleStateMachine machine) : base(manager, machine) { }

    public override void Enter()
    {
        // Check if battle should end
        if (battleManager.turnQueue.OnlyOneTeamRemaining())
        {
            stateMachine.ChangeState(new EndBattleState(battleManager, stateMachine));
            return;
        }

        Unit nextUnit = battleManager.turnQueue.GetNextUnit();
        if (nextUnit == null)
        {
            GD.Print("No unit ready to act. Accumulating AP...");
            stateMachine.ChangeState(this); // Stay in this state until a unit is ready
            return;
        }

        GD.Print($"It's {nextUnit.Stats.UnitName}'s turn! (Team {nextUnit.Team.TeamID})");

        if (nextUnit.Team.IsPlayerControlled)
            stateMachine.ChangeState(new PlayerTurnState(battleManager, stateMachine, nextUnit));
        else
            stateMachine.ChangeState(new EnemyTurnState(battleManager, stateMachine, nextUnit));
    }

    public override void Exit() { }
}
