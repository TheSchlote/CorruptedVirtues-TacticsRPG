using Godot;
using System;

public partial class BattleStateMachine : Node
{
    private BattleState currentState;
    private BattleManager battleManager;

    public void Initialize(BattleManager manager)
    {
        battleManager = manager;
    }

    public void ChangeState(BattleState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }
}
