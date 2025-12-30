using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class UnitBase : LoadAble, ICanBanMove
{
    public override void getMove(int dep, int x, int y)
    {
        canGoTo.Add((x, y));
        Queue<(int, int, int)> que=new Queue<(int, int, int)>();
        que.Enqueue((dep, x, y));
        while (que.Count > 0)
        {
            (dep, x, y) = que.Dequeue();
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
                    que.Enqueue((dep - 1, nx, ny));
                }
            }
        }
    }
    public override void TakeDamage(Piece e, int dmg, bool isPierce = false)
    {
        if (status != CardStatus.OnBoard) return;
        if (e == null || GameManager.Instance.curPlayer == player) dmg /= 3;
        if(TakeDamageModifier!=null)
        foreach (DamageModifier modifier in TakeDamageModifier.GetInvocationList())
            dmg = modifier(dmg, e, this);
        if (dmg <= 0) return;


        if (renderer is BoardPieceRenderer bprend) bprend.TakeDamageAnimation();
        if (!isPierce)
        {
            if (DF > dmg)
            {
                DF -= dmg;
                return;
            }
            else
            {
                dmg -= DF;
                DF = 0;
                EventManager.TriggerOnBreak(e, this);
            }
        }
        if (HP > dmg)
        {
            HP -= dmg;
            if(renderer!=null)pieceRenderer.UpdateData();
        }
        else
        {
            HP = 0;
            EventManager.TriggerOnKill(e, this);
            if(renderer!=null)pieceRenderer.UpdateData();
            OnDeath();
        }
    }
    public override async UniTask TakeAction()
    {
        if (canAct)
        {
            canAct = false;
            await Attack();
        }
    }
}

public class Master : UnitBase, ICanBanKnock
{
    public Master() : base()
    {
        maxHP = HP = 20;
        AT = 3;
        maxDF = DF = 1;
        RA = 2;
        ST = 0;
        canClimb = 1; canSwim = 1; canBanMagic = 0; canRide = 0;
        capacity = 1;
        cardName = "Master";
        cardDescription = "你可以在Master所在处或身旁召唤从者，Master也能够攻击，在关键时刻可以补一刀。但要小心，一旦Master被击杀，你会立即输掉游戏！保护好你的Master，并击杀其他玩家的Master以获得胜利。";
        sprite = GameManager.Instance.pieceAtlas.GetSprite("Master");
    }
    
    public override async UniTask OnDeath()
    {
        await base.OnDeath();
        if (GameManager.Instance.curPlayer == player)
            player.TurnEndTcs?.TrySetResult();
        GameManager.Instance.SetLose(player);
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
        canClimb = 1;canSwim = 1;canBanMagic = 0;canRide = 0;
        capacity = 1;
        cardName = "傀儡";
        cardDescription = "傀儡是Master的延伸，你可以像在Master身旁一样，在傀儡旁召唤从者。";
        sprite = GameManager.Instance.pieceAtlas.GetSprite("Golem");
    }
    public override async UniTask<bool> UseCard(Player usr)
    {
        if (usr.CommandCount <= 0) return false;

        List<(int, int)> buf = GameManager.Instance.tiles
            .Where(pii => pii.Value.onTile == null)
            .Select(pii => pii.Key)
            .ToList();

        if (buf.Count <= 0) return false;
        
        --usr.CommandCount;
        (xpos, ypos) = await usr.SelectPosition(buf);
        facing = await usr.SelectDirection(xpos, ypos);
        status = CardStatus.OnBoard;
        DealDamageModifier = usr.DealDamageModifier;
        TakeDamageModifier = usr.TakeDamageModifier;
        buffs = new List<Buff>(usr.buffs);
        usr.onBoardList.Add(this);

        GameObject go = GameObject.Instantiate(GameManager.Instance.BoardPiecePrefab);
        renderer = go.GetComponent<BoardPieceRenderer>();
        renderer.data = this;
        renderer.InitSprite();

        Tile targetTile = GameManager.Instance.GetTile(xpos, ypos);
        if (targetTile.onTile == null)
            { tile = targetTile; tile.onTile = this; }
        return true;
    }
}
