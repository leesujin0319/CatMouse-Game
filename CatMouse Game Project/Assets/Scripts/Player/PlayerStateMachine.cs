using System.Collections;
using UnityEngine;

public class PlayerStateMachine : StateMachine
{
    public Player player { get; }
    public PlayerMoveState playerMoveState { get; private set; }
    public PlayerRunState playerRunState { get; private set; }
    public PlayerBaseAttackState playerBaseAttackState { get; private set; }
    public PlayerFindingState playerFindingState {get; private set;} // 적 감지 > 먹기
    public PlayerHitState playerHitState { get; private set; }
    

    public  PlayerStateMachine(Player player)
    {
        this.player = player;
        playerMoveState = new PlayerMoveState(this);
        playerRunState = new PlayerRunState(this);
        playerBaseAttackState = new PlayerBaseAttackState(this);
        playerFindingState = new PlayerFindingState(this);
        playerHitState = new PlayerHitState(this);

    }
}