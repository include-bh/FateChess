using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Vehicle : LoadAble
{
}

public class Truck : Vehicle
{
    public Truck() : base()
    {
        maxHP = HP = 10;
        AT = 0;
        maxDF = DF = 2;
        RA = 0;
        ST = 3;
        canClimb = 0; canSwim = 1; canBanMagic = 0; canRide = 0;
        capacity = 3;
        cardName = "战车";
        cardDescription = "载具，可以装载3个从者，快速冲往前线。\n载具不会阻止其上从者行动，但也不会防止其上从者受到攻击。";
        sprite = GameManager.Instance.pieceAtlas.GetSprite("Truck");
    }
    public override void InitCard()
    {
        base.InitCard();
        maxHP = HP = 10;
        AT = 0;
        maxDF = DF = 2;
        RA = 0;
        ST = 3;
        canClimb = 0; canSwim = 1; canBanMagic = 0; canRide = 0;
    }

    public override void getMove(int dep, int x, int y)
    {
        canGoTo.Add((x, y));
        Queue<(int, int, int)> que = new Queue<(int, int, int)>();
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
                    if (newTile.onTile != null)
                    {
                        if (newTile.onTile.player != this.player) continue;
                    }
                    if (newTile.type == Terrain.Water && canSwim == 0) continue;
                    int newdep = dep - 1;
                    if (newTile.type == Terrain.Hill && canClimb == 0) newdep = 0;
                    que.Enqueue((newdep, nx, ny));
                }
            }
        }
    }
    public override async UniTask TakeAction()
    {
        if (buffs.Any(p => p is Stun)) return;
        bool ok = false;
        if (canAct)
        {
            foreach (Piece p in onLoad.OfType<Piece>())
                if (p.canRide > 0) ok = true;
        }
        if (ok == false)
        {
            if (player.CommandCount <= 0) return;
            --player.CommandCount;
        }
        else canAct = false;
        await Move();
    }
    public override async UniTask Move()
    {
        await base.Move();
        foreach (Piece p in onLoad.OfType<Piece>())
            (p.xpos, p.ypos) = (xpos, ypos);
    }
}

public class Glider : Vehicle
{
    public Glider():base()
    {
        maxHP = HP = 3;
        AT = 7;
        maxDF = DF = 0;
        RA = 0;
        ST = 0;
        canClimb = 0;canSwim = 0;canBanMagic = 0;canRide = 0;
        capacity = 3;
        cardName = "滑翔机";
        cardDescription = "载具，可以装载3个从者飞向前线。\n由于结构并不稳定，滑翔机只能在山地起飞，且行动后会坠毁，并对目标格上单位造成穿透伤害。";
        sprite = GameManager.Instance.pieceAtlas.GetSprite("Glider");
    }
    public override void InitCard()
    {
        base.InitCard();
        maxHP = HP = 5;
        AT = 7;
        maxDF = DF = 0;
        RA = 0;
        ST = 0;
        canClimb = 0; canSwim = 0; canBanMagic = 0; canRide = 0;
    }
    public override async UniTask<bool> UseCard(Player usr)
    {
        if (usr.CommandCount <= 0) return false;

        List<(int, int)> buf = GameManager.Instance.tiles
            .Where(pii => pii.Value.onTile == null && pii.Value.type == Terrain.Hill)
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
    public override async UniTask Attack()
    {
        canGoTo.Clear();
        foreach (var x in GameManager.Instance.tiles.Keys)
            canGoTo.Add(x);
        var (nx, ny) = await player.SelectPosition(new List<(int, int)>(canGoTo));
        xpos = nx;ypos = ny;
        pieceRenderer?.UpdatePosition();

        tile.onTile = null;
        foreach (Piece x in onLoad.OfType<Piece>())
            (x.xpos, x.ypos) = (nx, ny);

        Piece e = GameManager.Instance.GetTile(nx, ny).onTile;
        if (e != null)
        {
            if (e is LoadAble ve)
            {
                List<Piece> pbuf = ve.onLoad.OfType<Piece>().ToList();
                foreach (Piece x in pbuf)
                    DealDamage(x, AT, true);
            }
            DealDamage(e, AT, true);
        }
        OnDeath();
    }
    public override async UniTask OnDeath()
    {
        tile = GameManager.Instance.GetTile(xpos, ypos);
        List<Piece> buf = onLoad.OfType<Piece>().ToList();
        foreach (Piece p in buf)
        {
            if (tile.onTile == null && (p.canSwim > 0 || tile.type != Terrain.Water))
            {
                p.xpos = this.xpos;
                p.ypos = this.ypos;
                p.UpdateOnBoardPosition();
            }
            else p.ForceMove();
        }
        onLoad.Clear();
        tile = null;
        await base.OnDeath();
    }
    public override async UniTask TakeAction()
    {
        if (buffs.Any(p => p is Stun)) return;
        bool ok = false;
        if (canAct)
        {
            foreach (Piece p in onLoad.OfType<Piece>())
                if (p.canRide > 0) ok = true;
        }
        if (ok == false)
        {
            if (player.CommandCount <= 0) return;
            --player.CommandCount;
        }
        else canAct = false;
        await Attack();
    }
}
