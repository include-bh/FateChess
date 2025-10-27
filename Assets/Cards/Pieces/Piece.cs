using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class Piece : Card
{

    public static readonly int[] dx = { 1, 0, -1, -1, 0, 1, 0 };
    public static readonly int[] dy = { 0, 1, 1, 0, -1, -1, 0 };

    // Start is called before the first frame update
    public void Start()
    {

    }

    // Update is called once per frame
    public void Update()
    {
        transform.position = GameManager.Instance.GetPosition(xpos, ypos);
        transform.rotation = Quaternion.Euler(0,0,GameManager.Instance.GetRotation(facing));
        if (GameManager.Instance.playerAtlas != null && playerRend != null)
        {
            string bgName = $"Player{belong}";
            playerRend.sprite = GameManager.Instance.playerAtlas.GetSprite(bgName);
        }

        HPText.text = HP.ToString();
        ATText.text = AT.ToString();
        DFText.text = DF.ToString();
        RAText.text = RA.ToString();
        STText.text = ST.ToString();
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
    public int belong;
    public SpriteRenderer playerRend;
    [Header("战时")]
    public bool hasActed;
    public Weapon equip;

    /*
        public Transform HPpos, ATpos, DFpos, RApos, STpos;
        public EffectDFDown DFDownPrefab;
        public EffectDFBreak DFBreakPrefab;
        public EffectHPDown HPDownPrefab;
        public EffectBuff BuffPrefab;
        public EffectDebuff DebuffPrefab;
    */

    protected HashSet<(int, int)> canGoTo;
    protected List<(int,int)> canHit;

    protected virtual void dfs(int dep, int x, int y)
    {
        Tile curTile = GameManager.Instance.getTile(x, y);
        if (curTile.onTile == null)
            canGoTo.Add((x, y));
        else
        {
            if (curTile.onTile is Vehicle ve)
            {
                if (ve.belong == this.belong && ve.onLoad.Count < 3)
                    canGoTo.Add((x, y));
            }
            if (curTile.onTile is UnitBase ub)
            {
                if (ub.belong == this.belong && ub.onLoad.Count < 1)
                    canGoTo.Add((x, y));
            }
        }
        if (dep != 0)
        {
            for (int i = 0; i < 6; i++)
            {
                int nx = x + dx[i], ny = y + dy[i];
                Tile newTile = GameManager.Instance.getTile(nx, ny);
                if (newTile == null) continue;
                if (newTile.onTile != null)
                {
                    if (newTile.onTile.belong != this.belong) continue;
                    if (newTile.onTile is Vehicle ve && ve.onLoad.Count < 3) dfs(dep - 1, nx, ny);
                    else if (newTile.onTile is UnitBase ub && ub.onLoad.Count < 1) dfs(dep - 1, nx, ny);
                    else
                    {
                        if (newTile.type == Terrain.Water && canSwim == false) continue;
                        int newdep = dep - 1;
                        if (newTile.type == Terrain.Hill && canClimb == false) newdep = 0;
                        dfs(newdep, nx, ny);
                    }
                }
                else
                {
                    if (newTile.type == Terrain.Water && canSwim == false) continue;
                    int newdep = dep - 1;
                    if (newTile.type == Terrain.Hill && canClimb == false) newdep = 0;
                    dfs(newdep, nx, ny);
                }
            }
        }
    }

    public virtual void Move()
    {
        canGoTo.Clear();
        dfs(ST, xpos, ypos);
    }

    public virtual void Attack()
    {
        for(int i = -RA; i <= RA; i++)
        {
            for(int j = Math.Max(-RA, -i - RA); j <= Math.Min(RA, -i + RA); j++)
            {
                int nx = xpos + i;
                int ny = ypos + j;
                canHit.Add((nx, ny));
            }
        }
    }
    public void Damage(int x)
    {
        if (DF > x)
        {
            DF -= x;
        }
        else
        {
            x -= DF;
            DF = 0;
            if (HP > x)
            {
                HP -= x;
            }
            else
            {
                HP = 0;
                enabled = false;
            }
        }
    }
    public void RoundBegin()
    {
        DF = maxDF;
        hasActed = false;
    }

    
    public override void UseCard(Player usr)
    {
        
    }
}
