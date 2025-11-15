using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class UnitBase : LoadAble
{
    public override void dfsMove(int dep, int x, int y)
    {
        vis.Add((x, y));
        Tile curTile = GameManager.Instance.GetTile(x, y);
        if (curTile.onTile == null)
            canGoTo.Add((x, y));
        if (dep != 0)
        {
            for (int i = 0; i < 6; i++)
            {
                int nx = x + dx[i], ny = y + dy[i];
                Tile newTile = GameManager.Instance.GetTile(nx, ny);
                if (newTile == null) continue;
                dfsMove(dep - 1, nx, ny);
            }
        }
    }
    public override async UniTask Move()
    {

    }
    public override string GetDescription()
    {
        return cardDescription;
    }
}

public class Master : UnitBase
{
    public Master():base()
    {
        maxHP = HP = 20;
        AT = 3;
        maxDF = DF = 1;
        RA = 2;
        ST = 0;
        canClimb = false;canSwim = false;canBanMagic = false;canRide = false;
        cardName = "Master";
        cardDescription = "你可以在Master身旁召唤从者，Master也能够攻击，在关键时刻可以补一刀。但要小心，一旦Master被击杀，你会立即输掉游戏！保护好你的Master，并击杀其他玩家的Master以获得胜利。";
        sprite = GameManager.Instance.pieceAtlas.GetSprite("Master");
    }
}
public class Golem : UnitBase
{
    public Golem():base()
    {
        maxHP = HP = 10;
        AT = 2;
        maxDF = DF = 1;
        RA = 2;
        ST = 0;
        canClimb = false;canSwim = false;canBanMagic = false;canRide = false;
        cardName = "傀儡";
        cardDescription = "傀儡是Master的延伸，你可以像在Master身旁一样，在傀儡旁召唤从者。";
        sprite = GameManager.Instance.pieceAtlas.GetSprite("Golem");
    }
}
