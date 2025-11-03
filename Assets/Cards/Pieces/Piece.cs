using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
public class Piece : Card
{

    public static readonly int[] dx = { 1, 0, -1, -1, 0, 1, 0 };
    public static readonly int[] dy = { 0, 1, 1, 0, -1, -1, 0 };

    // Start is called before the first frame update
    public override void Start()
    {
        BoardRend.enabled = false;
        BoardRend.sprite = UIRend.sprite;
    }

    // Update is called once per frame
    public override void Update()
    {
        

        
    }

    void UpdatePosition()
    {
        transform.position = GameManager.Instance.GetPosition(xpos, ypos);
        transform.rotation = Quaternion.Euler(0, 0, GameManager.Instance.GetRotation(facing));
    }
    void UpdateTeam()
    {
        if (GameManager.Instance.playerAtlas != null && PlayerRend != null)
        {
            string bgName = $"Player{belong.id}";
            PlayerRend.sprite = GameManager.Instance.playerAtlas.GetSprite(bgName);
        }
    }
    void UpdateData()
    {
        HPText.text = HP.ToString();
        DFText.text = DF.ToString();
        if (ST != 0) STText.text = ST.ToString();
        if (AT != 0)
        {
            ATText.text = AT.ToString();
            RAText.text = RA.ToString();
        }
    }

    [Header("基础属性")]
    public int maxHP;
    public int HP;
    public TextMeshPro HPText;
    public int AT;
    public TextMeshPro ATText;
    public int maxDF;
    public int DF;
    public TextMeshPro DFText;
    public int RA;
    public TextMeshPro RAText;
    public int ST;
    public TextMeshPro STText;
    [Header("位置")]
    public int xpos;
    public int ypos;
    public int facing;
    [Header("技能")]
    public bool canClimb;
    public bool canSwim;
    public bool canBanMagic;
    [Header("归属")]
    public Player belong;
    public SpriteRenderer BoardRend;
    public SpriteRenderer PlayerRend;
    [Header("战时")]
    public bool hasActed;

    /*
        public Transform HPpos, ATpos, DFpos, RApos, STpos;
        public EffectDFDown DFDownPrefab;
        public EffectDFBreak DFBreakPrefab;
        public EffectHPDown HPDownPrefab;
        public EffectBuff BuffPrefab;
        public EffectDebuff DebuffPrefab;
    */

    public HashSet<(int, int)> vis, canGoTo, canHit;

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
                    if (newTile.onTile.belong != this.belong) continue;
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

    public virtual void Move()
    {
        vis.Clear();canGoTo.Clear();
        dfsMove(ST, xpos, ypos);
        var (nx, ny) = belong.SelectPosition(new List<(int, int)>(canGoTo));
        xpos = nx;ypos = ny;
        var nf = belong.SelectDirection();
        facing = nf;
        UpdatePosition();
    }

    public virtual void dfsHit(int dep, int x, int y)
    {
        if (vis.Contains((x, y))) return;
        vis.Add((x, y));
        Tile curTile = GameManager.Instance.GetTile(x, y);
        if (curTile != null)
            canHit.Add((x, y));
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
    public virtual void Hit()
    {
        vis.Clear(); canHit.Clear();
        dfsHit(RA - 1, xpos + dx[facing], ypos + dy[facing]);
        dfsHit(RA - 1, xpos + dx[(facing + 1) % 6], ypos + dy[(facing + 1) % 6]);
        Piece e = belong.SelectTarget(new List<(int, int)>(canHit));
        if (e != null) Attack(e);
    }
    
    public virtual void Attack(Piece e)
    {
        EventManager.TriggerOnAttack(this, e);
        e.TakeDamage(this,AT);
    }

    public virtual void TakeDamage(Piece e,int x)
    {
        if (DF > x)
        {
            DF -= x;
        }
        else
        {
            x -= DF;
            DF = 0;
            EventManager.TriggerOnBreak(e,this);
            if (HP > x)
            {
                HP -= x;
            }
            else
            {
                HP = 0;
                EventManager.TriggerOnKill(e,this);
                OnDeath();
            }
        }
    }

    public virtual void OnDeath()
    {
        Tile tile = GameManager.Instance.GetTile(xpos, ypos);
        tile.onTile = null;
        gameObject.SetActive(false);
        GameManager.Instance.DiscardCard(this);
    }

    public virtual void RoundBegin()
    {
        DF = maxDF;
        hasActed = false;
    }
    
    public void ForceMove()
    {
        Tile curTile = GameManager.Instance.GetTile(xpos, ypos);
        if (curTile.type != Terrain.Water || canSwim)
        { curTile.onTile = this; return; }
        curTile.onTile = null;
        List<Tile> buf = new List<Tile>();
        for (int i = 0; i < 6; i++)
        {
            Tile t = GameManager.Instance.GetTile(xpos + dx[i], ypos + dy[i]);
            if (t != null) buf.Add(t);
        }
        buf = buf.OrderBy(tile =>
        {
            if (canSwim && tile.type == Terrain.Water) return 0;
            if (canClimb && tile.type == Terrain.Hill) return canSwim ? 1 : 0;
            if (tile.type == Terrain.Plain) return canSwim ? (canClimb ? 2 : 1) : (canClimb ? 1 : 0);
            if (tile.type == Terrain.Hill) return canSwim ? 2 : 1;
            return 2;
        }
        ).ToList();

        for (int i = 0; i < buf.Count; i++)
        {
            if (!canSwim && buf[i].type == Terrain.Water) continue;
            if (buf[i].onTile == null)
            {
                buf[i].onTile = this;
                UpdatePosition();
                return;
            }
        }
        OnDeath();
    }

    
    public override void UseCard(Player usr)
    {
        UIRend.enabled = false;
        BoardRend.enabled = true;
    }
}
