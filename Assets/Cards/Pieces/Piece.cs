using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;

[System.Serializable]
public class Piece : Card
{
    public static readonly int[] dx = { 1, 0, -1, -1, 0, 1, 0 };
    public static readonly int[] dy = { 0, 1, 1, 0, -1, -1, 0 };

    public Piece() : base()
    {
        canAct = true;
        player = null;
        tile = null;
    }
    public int maxHP, HP, AT, maxDF, DF, RA, ST;
    public int xpos, ypos, facing;
    public bool canClimb, canSwim, canBanMagic, canRide;
    public bool canAct;

    [NonSerialized]public Player player;
    public Tile tile;


    public void UpdateOnBoardPosition()
    {
        if (tile == null) return;
        if (xpos == tile.xpos && ypos == tile.ypos) return;
        tile.onTile = null;
        tile = GameManager.Instance.GetTile(xpos, ypos);
        tile.onTile = this;
    }
    /*
        public Transform HPpos, ATpos, DFpos, RApos, STpos;
        public EffectDFDown DFDownPrefab;
        public EffectDFBreak DFBreakPrefab;
        public EffectHPDown HPDownPrefab;
        public EffectBuff BuffPrefab;
        public EffectDebuff DebuffPrefab;
    */

    [NonSerialized]public HashSet<(int, int)> vis, canGoTo; 
    [NonSerialized]public HashSet<Piece> canHit;

    public virtual void dfsMove(int dep, int x, int y)
    {
        if (vis.Contains((x, y))) return;
        vis.Add((x, y));
        Tile curTile = GameManager.Instance.GetTile(x, y);
        if (curTile.onTile == null)
            canGoTo.Add((x, y));
        else if (curTile.onTile is LoadAble ve && ve.onLoad.Count < ve.capacity)
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
                    if (newTile.onTile is LoadAble ve && ve.onLoad.Count < ve.capacity) dfsMove(dep - 1, nx, ny);
                    else
                    {
                        if (newTile.type == Terrain.Water && canSwim == false) continue;
                        int newdep = dep - 1;
                        if (newTile.type == Terrain.Hill && canClimb == false) newdep = 0;
                        dfsMove(newdep, nx, ny);
                    }
                }
                else
                {
                    if (newTile.type == Terrain.Water && canSwim == false) continue;
                    int newdep = dep - 1;
                    if (newTile.type == Terrain.Hill && canClimb == false) newdep = 0;
                    dfsMove(newdep, nx, ny);
                }
            }
        }
    }

    public virtual async Task Move()
    {
        vis.Clear(); canGoTo.Clear();
        dfsMove(ST, xpos, ypos);

        var (nx, ny) = await player.SelectPosition(new List<(int, int)>(canGoTo));
        xpos = nx;ypos = ny;
        UpdateOnBoardPosition();

        var nf = await player.SelectDirection(xpos,ypos);
        facing = nf;

        renderer.UpdateSprite();
    }

    public virtual void dfsHit(int dep, int x, int y)
    {
        if (vis.Contains((x, y))) return;
        vis.Add((x, y));
        Tile curTile = GameManager.Instance.GetTile(x, y);
        if (curTile != null)
        {
            if (curTile.onTile != null)
            {
                canHit.Add(curTile.onTile);
                if (curTile.onTile is LoadAble ve)
                    foreach (Piece p in ve.onLoad) canHit.Add(p);
            }
        }
        if (dep != 0)
        {
            for (int i = 0; i < 6; i++)
            {
                int nx = x + dx[i], ny = y + dy[i];
                Tile newTile = GameManager.Instance.GetTile(nx, ny);
                dfsHit(dep - 1, nx, ny);
            }
        }
    }
    public virtual async Task Attack()
    {
        vis.Clear(); canHit.Clear();
        dfsHit(RA - 1, xpos + dx[facing], ypos + dy[facing]);
        dfsHit(RA - 1, xpos + dx[(facing + 1) % 6], ypos + dy[(facing + 1) % 6]);
        Piece e = await player.SelectTarget(new List<Piece>(canHit));
        if (e != null) DealDamage(e, AT);
    }

    public virtual void DealDamage(Piece e, int dmg, bool isPierce = false)
    {
        EventManager.TriggerOnAttack(this, e);
        e.TakeDamage(this, dmg, isPierce);
    }

    public virtual void TakeDamage(Piece e, int dmg, bool isPierce = false)
    {
        if (dmg < 0) dmg = 0;

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
        }
        else
        {
            HP = 0;
            EventManager.TriggerOnKill(e, this);
            OnDeath();
        }
        
    }

    public virtual void OnDeath()
    {

        tile.onTile = null;
        GameManager.Instance.DiscardCard(this);
    }

    public virtual void OnTurnBegin()
    {
        DF = maxDF;
        canAct = true;
    }

    public void ForceMove()
    {
        List<Tile> buf = new List<Tile>();
        for (int i = 0; i < 6; i++)
        {
            Tile t = GameManager.Instance.GetTile(xpos + dx[i], ypos + dy[i]);
            if (t != null)
            {
                if (canSwim || t.type != Terrain.Water)
                    buf.Add(t);
            }
        }
        buf = buf.OrderBy(til =>
        {
            if (canSwim && til.type == Terrain.Water) return 0 + UnityEngine.Random.Range(0, 6);
            if (canClimb && til.type == Terrain.Hill) return (canSwim ? 1 : 0) + UnityEngine.Random.Range(0, 6);
            if (til.type == Terrain.Plain) return (canSwim ? (canClimb ? 2 : 1) : (canClimb ? 1 : 0)) + UnityEngine.Random.Range(0, 6);
            if (til.type == Terrain.Hill) return (canSwim ? 2 : 1) + UnityEngine.Random.Range(0, 6);
            return 2 + UnityEngine.Random.Range(0, 6);
        }
        ).ToList();

        if (buf.Count > 0)
        {
            (xpos, ypos) = (buf[0].xpos, buf[0].ypos);
            UpdateOnBoardPosition();
            renderer.UpdateSprite();
        }
        else OnDeath();
    }

    public override async Task UseCard(Player usr)
    {
        HashSet<(int, int)> buf = new HashSet<(int, int)>();
        foreach (Piece p in usr.onBoardList)
            if (p is UnitBase)
                for (int i = 0; i < 7; i++)
                {
                    Tile t = GameManager.Instance.GetTile(p.xpos + dx[i], p.ypos + dy[i]);
                    if (t == null) continue;
                    if (t.onTile == null) buf.Add((t.xpos, t.ypos));
                    if(this is ICanOnLoad)
                    {
                        if (t.onTile is LoadAble ve)
                        {
                            if (ve.player == usr && ve.onLoad.Count < ve.capacity)
                                buf.Add((t.xpos, t.ypos));
                        }
                    }
                }

        (xpos, ypos) = await usr.SelectPosition(buf.ToList());
        facing = await usr.SelectDirection(xpos, ypos);
        Tile targetTile = GameManager.Instance.GetTile(xpos, ypos);
        if (targetTile.onTile == null)
        {
            tile = targetTile; tile.onTile = this;
            GameObject go = GameObject.Instantiate(GameManager.Instance.BoardPiecePrefab);
            renderer = go.GetComponent<BoardPieceRenderer>();
            renderer.data = this;
            renderer.UpdateSprite();
        }
        else if (this is ICanOnLoad load && targetTile.onTile is LoadAble ve)
            ve.onLoad.Add(load);
    }

    public override string GetDescription()
    {
        string res = cardDescription;
        if (canClimb) res += "\n【攀爬】可以通过山地。";
        if (canSwim) res += "\n【涉水】可以进入水域。";
        if (canBanMagic) res += "\n【对魔力】可以停止Caster的连锁攻击。";
        if (canRide) res += "\n【驾驶】可以不消耗令咒使所在载具行动。";
        return res;
    }
}

public interface ICanOnLoad
{
}
public interface ICanBanMove
{
}
public interface ICanBanWind
{
}