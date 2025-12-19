using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class AIPlayer : Player
{
    private readonly float attackBias;
    private readonly float defenseBias;

    // 上下文字段（仅用于 Select 方法，不修改游戏状态）
    private Card currentUsingCard = null;
    private int tarx = -999, tary = -999, tarf = -1;
    private Piece tar;

    public AIPlayer(int team, float attackBias, float defenseBias) : base(team)
    {
        this.attackBias = attackBias;
        this.defenseBias = defenseBias;
    }

    public override async UniTask OnMyTurn(int initialCommandCount)
    {
        GameManager.Instance.curPlayer = this;
        CommandCount = initialCommandCount;

        foreach (Piece x in onBoardList)
        {
            x.OnTurnBegin();
            x.pieceRenderer.UpdateData();
        }
/*

        // 1. 胜利判定：添加棋盘块（需有 WuXie 保底）
        if (await TryWinCondition()) return;

        // 2. 使用“无中生有”
        if (HasCard("无中生有"))
        {
            await UseCardWithContext(GetCard("无中生有"));
        }

        // 3. 策略决策
        bool shouldAttack = DecidePrimaryStrategy();

        if (shouldAttack)
        {
            await ExecuteAttackBehavior();
            await ExecuteDefenseBehavior();
        }
        else
        {
            await ExecuteDefenseBehavior();
            await ExecuteAttackBehavior();
        }

        // 4. 普通从者行动（不消耗令咒）
        await ExecuteNormalServantActions();

        // 5. 部署逻辑（卡牌内部扣令咒）
        await ExecuteDeploymentLogic();
        */
    }

}