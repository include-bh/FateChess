using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class AIPlayer : Player
{
    // Start is called before the first frame update
    int AttackRate, DefenceRate;
    public AIPlayer(int t = 0, int Ad = 1, int Dd = 1) : base(t)
    {
        AttackRate = Ad;
        DefenceRate = Dd;
    }

    public override async UniTask<(int, int)> SelectPosition(List<(int, int)> PosSet)
    {
        return PosSet[0];
    }

    public override async UniTask<int> SelectDirection(int xpos, int ypos, bool rot=false)
    {
        return 0;
    }
    
    public override async UniTask<Piece> SelectTarget(List<Piece> TargetSet)
    {
        return null;
    }
    public override async UniTask OnMyTurn(int cmd)
    {
        GameManager.Instance.curPlayer = this;
        CommandCount = cmd;
        foreach (Piece x in onBoardList)
        {
            x.OnTurnBegin();
            x.pieceRenderer.UpdateData();
        }

    }
}
