using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

    public override async Task<(int, int)> SelectPosition(List<(int, int)> PosSet)
    {
        return PosSet[0];
    }

    public override async Task<int> SelectDirection(int xpos, int ypos)
    {
        return 0;
    }
    
    public override async Task<Piece> SelectTarget(List<Piece> TargetSet)
    {
        return null;
    }

}
