using Godot;
using System;

public abstract class BattleState
{
    protected BattleManager battleManager;
    protected BattleStateMachine stateMachine;

    public BattleState(BattleManager manager, BattleStateMachine machine)
    {
        battleManager = manager;
        stateMachine = machine;
    }

    public abstract void Enter();
    public abstract void Exit();
}
