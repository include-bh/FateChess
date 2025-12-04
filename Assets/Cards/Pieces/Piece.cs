using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
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
        tile = null;
        canGoTo = new HashSet<(int, int)>();
        canHit = new HashSet<Piece>();
    }
    public override void InitCard()
    {
        base.InitCard();
        canAct = true;
        tile = null;
        canGoTo.Clear();
        canHit.Clear();
    }
    public int maxHP, HP, AT, maxDF, DF, RA, ST;
    public bool isPierce = false;
    public int xpos, ypos, facing;
    public int canClimb, canSwim, canBanMagic, canRide;
    public bool canAct;

    [NonSerialized]public Tile tile;

    public DamageModifier OutgoingDamageModifier;
    public DamageModifier IncomingDamageModifier;
    public List<Buff> buffs;

    public override CardRenderer renderer
    {
        get => rend;
        set
        {
            if (value == null) rend = null;
            else if (value is IPieceRenderer) rend = value;
            else throw new ArgumentException();
        }
    }
    public new IPieceRenderer pieceRenderer
    {
        get => (IPieceRenderer)rend;
        set => rend = (CardRenderer)value;
    }

    public void UpdateOnBoardPosition()
    {
        if (tile == null) return;
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

    [NonSerialized]public HashSet<(int, int)> canGoTo; 
    [NonSerialized]public HashSet<Piece> canHit;

    public virtual void getMove(int dep, int x, int y)
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
                        if (newTile.onTile is LoadAble ve && ve.onLoad.Count < ve.capacity) que.Enqueue((dep-1, nx, ny));
                        else
                        {
                            if (newTile.type == Terrain.Water && canSwim == 0) continue;
                            int newdep = dep - 1;
                            if (newTile.type == Terrain.Hill && canClimb == 0) newdep = 0;
                            que.Enqueue((newdep, nx, ny));
                        }
                    }
                    else
                    {
                        if (newTile.type == Terrain.Water && canSwim == 0) continue;
                        int newdep = dep - 1;
                        if (newTile.type == Terrain.Hill && canClimb == 0) newdep = 0;
                        que.Enqueue((newdep, nx, ny));
                    }
                }
            }
        }
    }

    public virtual async UniTask Move()
    {
        canGoTo.Clear();
        getMove(ST, xpos, ypos);

        var (nx, ny) = await player.SelectPosition(new List<(int, int)>(canGoTo));
        xpos = nx;ypos = ny;
        UpdateOnBoardPosition();
        pieceRenderer?.UpdatePosition();

        var nf = await player.SelectDirection(xpos,ypos);
        facing = nf;
        pieceRenderer?.UpdateRotation();
    }

    public virtual void getHit(int dep, int x, int y)
    {
        Queue<(int, int, int)> que=new Queue<(int, int, int)>();
        que.Enqueue((dep, x, y));
        while (que.Count > 0)
        {
            (dep, x, y) = que.Dequeue();
            Tile curTile = GameManager.Instance.GetTile(x, y);
            if (curTile != null)
            {
                if (curTile.onTile != null && curTile.onTile.player != this.player)
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
                    que.Enqueue((dep-1, x, y));
                }
            }
        }
    }
    public virtual async UniTask Attack()
    {
        canHit.Clear();
        getHit(RA - 1, xpos + dx[facing], ypos + dy[facing]);
        getHit(RA - 1, xpos + dx[(facing + 1) % 6], ypos + dy[(facing + 1) % 6]);
        Piece e = await player.SelectTarget(new List<Piece>(canHit));
        if (e != null) DealDamage(e, AT, isPierce);
    }

    public virtual void DealDamage(Piece e, int dmg, bool isPierce = false)
    {
        EventManager.TriggerOnAttack(this, e);
        GameObject go = GameObject.Instantiate(GameManager.Instance.AttackEffectPrefab);

        AttackEffect ef = go.GetComponent<AttackEffect>();
        ef.start = GameManager.GetPosition(xpos, ypos);
        ef.end = GameManager.GetPosition(e.xpos, e.ypos);
        ef.PlayAnimation();

        e.TakeDamage(this, dmg, isPierce);
    }

    public virtual void TakeDamage(Piece e, int dmg, bool isPierce = false)
    {
        if (dmg <= 0) return;

        if (e == null && GameManager.Instance.curPlayer == player) dmg /= 3;

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
            pieceRenderer.UpdateData();
        }
        else
        {
            HP = 0;
            EventManager.TriggerOnKill(e, this);
            pieceRenderer.UpdateData();
            OnDeath();
        }
    }
    
    public virtual void TakeHeal(int hp)
    {
        HP = Math.Min(HP + hp, maxHP);
        pieceRenderer.UpdateData();
    }

    public virtual void OnDeath()
    {
        if (tile != null) tile.onTile = null;
        player.onBoardList.Remove(this);
        GameManager.Instance.DiscardCard(this);
        if (renderer is BoardRenderer brend)
            brend.LeaveAnimation();
        GameObject.Destroy(renderer.gameObject);
        renderer = null;
    }

    public virtual void OnTurnBegin()
    {
        DF = maxDF;
        canAct = true;
    }
    public virtual async UniTask TakeAction()
    {
        if (canAct)
        {
            await Move();
            await Attack();
            canAct = false;
        }
    }

    public void ForceMove(int dir = 6)
    {
        if (this is ICanBanMove) return;
        List<Tile> buf = new List<Tile>();
        for (int i = 0; i < 6; i++)
        {
            if (dir < 6 && (i == dir || i == dir + 3 || i == dir - 3)) continue;
            Tile t = GameManager.Instance.GetTile(xpos + dx[i], ypos + dy[i]);
            if (t != null)
            {
                if (canSwim>0 || t.type != Terrain.Water)
                    buf.Add(t);
            }
        }
        buf = buf.OrderBy(til =>
        {
            if (canSwim>0 && til.type == Terrain.Water) return 0 + UnityEngine.Random.Range(0, 6);
            if (canClimb>0 && til.type == Terrain.Hill) return (canSwim>0 ? 1 : 0) + UnityEngine.Random.Range(0, 6);
            if (til.type == Terrain.Plain) return (canSwim>0 ? (canClimb>0 ? 2 : 1) : (canClimb>0 ? 1 : 0)) + UnityEngine.Random.Range(0, 6);
            if (til.type == Terrain.Hill) return (canSwim>0 ? 2 : 1) + UnityEngine.Random.Range(0, 6);
            return 2 + UnityEngine.Random.Range(0, 6);
        }
        ).ToList();

        if (buf.Count > 0)
        {
            (xpos, ypos) = (buf[0].xpos, buf[0].ypos);
            UpdateOnBoardPosition();
            pieceRenderer?.UpdatePosition();
        }
        else OnDeath();
    }
    public void Knockback(int dir)
    {
        if (this is ICanBanMove) return;
        int nx = xpos + dx[dir], ny = ypos + dy[dir];
        Tile t = GameManager.Instance.GetTile(nx, ny);
        (xpos, ypos) = (nx, ny);
        pieceRenderer?.UpdatePosition();
        if (t == null) OnDeath();
        else if (t.type == Terrain.Water && canSwim == 0) OnDeath();
        else UpdateOnBoardPosition();
    }

    public override async UniTask<bool> UseCard(Player usr)
    {
        if (usr.CommandCount <= 0) return false;

        HashSet<(int, int)> buf = new HashSet<(int, int)>();
        foreach (Piece p in usr.onBoardList)
            if (p is UnitBase)
                for (int i = 0; i < 7; i++)
                {
                    Tile t = GameManager.Instance.GetTile(p.xpos + dx[i], p.ypos + dy[i]);
                    if (t == null) continue;
                    if (t.onTile == null)
                    {
                        if (canSwim > 0 || t.type != Terrain.Water)
                            buf.Add((t.xpos, t.ypos));
                    }
                    else if (t.onTile is LoadAble ve && this is ICanOnLoad)
                    {
                        if (ve.player == usr && ve.onLoad.Count < ve.capacity)
                            buf.Add((t.xpos, t.ypos));
                    }
                }
        if (buf.Count <= 0) return false;

        --usr.CommandCount;
        (xpos, ypos) = await usr.SelectPosition(buf.ToList());
        facing = await usr.SelectDirection(xpos, ypos);
        status = CardStatus.OnBoard;
        usr.onBoardList.Add(this);

        GameObject go = GameObject.Instantiate(GameManager.Instance.BoardPiecePrefab);
        renderer = go.GetComponent<BoardPieceRenderer>();
        renderer.data = this;
        renderer.InitSprite();

        Tile targetTile = GameManager.Instance.GetTile(xpos, ypos);
        if (targetTile.onTile == null)
            { tile = targetTile; tile.onTile = this; }
        else if (targetTile.onTile is LoadAble ve && this is ICanOnLoad load)
            { ve.onLoad.Add(load); renderer.gameObject.SetActive(false); }

        return true;
    }

    public override string GetDescription()
    {
        string res = cardDescription;
        if (canClimb>0) res += "\n【攀爬】可以通过山地。";
        if (canSwim>0) res += "\n【涉水】可以进入水域。";
        if (canBanMagic>0) res += "\n【对魔力】可以停止Caster的连锁攻击。";
        if (canRide>0) res += "\n【驾驶】载具上拥有含有此属性的从者时，可以不消耗令咒行动一次。";
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