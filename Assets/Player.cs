using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class Player
{
    private int cmdcnt = 0;
    public int CommandCount {
        get { return cmdcnt; }
        set {
            cmdcnt = value;
            if (UIManager.Instance.curPlayer == this)
                UIManager.Instance.UpdateCommandCount();
        }
    }
    public List<Card> hand = new List<Card>();
    public int id, team;
    public bool dead = false;
    public Master master=null;
    public List<Piece> onBoardList=new List<Piece>();


    public DamageModifier DealDamageModifier;
    public DamageModifier TakeDamageModifier;
    public List<Buff> buffs;

    public Player(int t = 0)
    {
        buffs = new List<Buff>();
        DealDamageModifier = null;
        TakeDamageModifier = null;
        team = t;
    }

    
    public UniTaskCompletionSource TurnEndTcs;
    public virtual async UniTask OnMyTurn(int cmd)
    {
        GameManager.Instance.curPlayer = this;
        UIManager.Instance.SetCurPlayer(this);
        CommandCount = cmd;
        foreach (Piece x in onBoardList)
        {
            x.OnTurnBegin();
            x.pieceRenderer.UpdateData();
        }

        UIManager.Instance.UpdateCommandCount();
        UIManager.Instance.UpdateHandCard();

        TurnEndTcs = new UniTaskCompletionSource();

        await TurnEndTcs.Task;
    }

    public UniTaskCompletionSource<(int,int)> PositionTcs;
    public virtual async UniTask<(int, int)> SelectPosition(List<(int, int)> PosSet)
    {
        UIManager.Instance.SwitchToSelectUI();
        UIManager.Instance.FinishButton.gameObject.SetActive(false);

        PositionTcs = new UniTaskCompletionSource<(int, int)>();

        List<SelectPositionTag> PositionTags=new List<SelectPositionTag>();
        foreach (var (x, y) in PosSet)
        {
            Vector2 pos = GameManager.GetPosition(x, y);
            GameObject go = GameObject.Instantiate(GameManager.Instance.SelectPositionTagPrefab, pos, Quaternion.identity);
            SelectPositionTag tag = go.GetComponent<SelectPositionTag>();
            tag.xpos = x; tag.ypos = y; tag.player = this;
            PositionTags.Add(tag);
            go.SetActive(true);
        }
        GameManager.Instance.raycaster.eventMask = LayerMask.GetMask("SelectTag");

        var (resx, resy) = await PositionTcs.Task;
        
        foreach (var tag in PositionTags)
            GameObject.Destroy(tag.gameObject);
        PositionTags.Clear();
        GameManager.Instance.raycaster.eventMask = LayerMask.GetMask("Piece");
        
        UIManager.Instance.SwitchToNormalUI();
        return (resx,resy);
    }


    public UniTaskCompletionSource<int> DirectionTcs;
    public virtual async UniTask<int> SelectDirection(int xpos, int ypos, bool rot = false)
    {
        UIManager.Instance.SwitchToSelectUI();
        UIManager.Instance.FinishButton.gameObject.SetActive(false);
        
        DirectionTcs = new UniTaskCompletionSource<int>();
        Vector2 pos = GameManager.GetPosition(xpos, ypos);
        
        List<SelectDirectionTag> DirectionTags = new List<SelectDirectionTag>();
        for (int i = 0; i < 6; i++)
        {
            GameObject go = GameObject.Instantiate(GameManager.Instance.SelectDirectionTagPrefab, pos, Quaternion.Euler(0, 0, 60f * i - (rot ? 30f : 0)));
            SelectDirectionTag tag = go.GetComponent<SelectDirectionTag>();
            tag.xpos = xpos; tag.ypos = ypos; tag.facing = i; tag.player = this;
            DirectionTags.Add(tag);
            go.SetActive(true);
        }
        GameManager.Instance.raycaster.eventMask = LayerMask.GetMask("SelectTag");

        var res = await DirectionTcs.Task;

        foreach (var tag in DirectionTags)
            GameObject.Destroy(tag.gameObject);
        DirectionTags.Clear();
        GameManager.Instance.raycaster.eventMask = LayerMask.GetMask("Piece");

        UIManager.Instance.SwitchToNormalUI();
        return res;
    }

    public UniTaskCompletionSource TargetTcs;
    public virtual async UniTask<Piece> SelectTarget(List<Piece> TargetSet)
    {
        if (TargetSet.Count == 0) return null;
        UIManager.Instance.SwitchToSelectUI();
        UIManager.Instance.FinishButton.gameObject.SetActive(true);

        PositionTcs = new UniTaskCompletionSource<(int, int)>();
        TargetTcs = new UniTaskCompletionSource();

        List<(int, int)> PosSet = TargetSet.Select(tile => (tile.xpos, tile.ypos)).ToList();

        List<SelectPositionTag> PositionTags=new List<SelectPositionTag>();
        foreach (var (x, y) in PosSet)
        {
            Vector2 pos = GameManager.GetPosition(x, y);
            GameObject go = GameObject.Instantiate(GameManager.Instance.SelectPositionTagPrefab, pos, Quaternion.identity);
            SelectPositionTag tag = go.GetComponent<SelectPositionTag>();
            tag.xpos = x; tag.ypos = y; tag.player = this;
            PositionTags.Add(tag);
            go.SetActive(true);
        }
        GameManager.Instance.raycaster.eventMask = LayerMask.GetMask("SelectTag");

        UIManager.Instance.FinishSelect += () => { TargetTcs?.TrySetResult(); };

        var tasks = new[] { PositionTcs.Task, TargetTcs.Task };
        int win = await UniTask.WhenAny(tasks);
        UIManager.Instance.ClearFinish();
            

        foreach (var tag in PositionTags)
            GameObject.Destroy(tag.gameObject);
        PositionTags.Clear();
        GameManager.Instance.raycaster.eventMask = LayerMask.GetMask("Piece");
        UIManager.Instance.SwitchToNormalUI();

        if (win == 1)return null;

        var (resx, resy) = await PositionTcs.Task;
        Tile tile = GameManager.Instance.GetTile(resx, resy);
        Piece tar = tile.onTile;

        GameManager.Instance.raycaster.eventMask = LayerMask.GetMask("Piece");
        UIManager.Instance.SwitchToNormalUI();

        if (tar is LoadAble ve)
        {
            List<Piece> buf = new List<Piece>();
            if (TargetSet.Contains(ve)) buf.Add(ve);
            foreach (Piece p in ve.onLoad.OfType<Piece>())
                if (TargetSet.Contains(p)) buf.Add(p);
            return await SelectTargetOnLoad(ve, buf);
        }
        else return tar;
    }
    public virtual async UniTask<Piece> SelectTargetOnLoad(LoadAble ve,List<Piece> TargetSet)
    {
        if (TargetSet.Count == 0) return null;
        //if (TargetSet.Count == 1) return TargetSet[0];

        UIManager.Instance.SwitchToSelectUI();
        UIManager.Instance.FinishButton.gameObject.SetActive(false);

        DirectionTcs = new UniTaskCompletionSource<int>();
        Vector2 pos = GameManager.GetPosition(ve.xpos, ve.ypos);

        List<SelectDirectionTag> DirectionTags = new List<SelectDirectionTag>();
        List<BoardPieceRenderer> rendbuf = new List<BoardPieceRenderer>();


        for (int i = 0; i < TargetSet.Count; i++)
        {
            GameObject go = GameObject.Instantiate(GameManager.Instance.SelectDirectionTagPrefab, pos, Quaternion.Euler(0, 0, 60f * i));
            SelectDirectionTag tag = go.GetComponent<SelectDirectionTag>();
            tag.xpos = ve.xpos; tag.ypos = ve.ypos; tag.facing = i; tag.player = this;
            DirectionTags.Add(tag);
            go.SetActive(true);

            Vector2 pos2 = pos + GameManager.SelectTargetPos[i]*1.2f;
            GameObject go2 = GameObject.Instantiate(GameManager.Instance.BoardPiecePrefab, pos2, Quaternion.identity);
            BoardPieceRenderer rend = go2.GetComponent<BoardPieceRenderer>();
            rend.data = TargetSet[i];
            rend.InitSpriteSelector();
            rendbuf.Add(rend);
        }

        GameManager.Instance.raycaster.eventMask = LayerMask.GetMask("SelectTag");

        var res = await DirectionTcs.Task;

        foreach (var tag in DirectionTags)
            GameObject.Destroy(tag.gameObject);
        DirectionTags.Clear();
        foreach (var tag in rendbuf)
            GameObject.Destroy(tag.gameObject);
        rendbuf.Clear();

        GameManager.Instance.raycaster.eventMask = LayerMask.GetMask("Piece");
        UIManager.Instance.SwitchToNormalUI();

        return TargetSet[res];
    }


    public async UniTask UseCard(int id)
    {
        Card used = hand[id];
        if (await used.UseCard(this))
            hand[id] = null;
        await UpdateHandCard();
    }

    public async UniTask UpdateHandCard()
    {
        List<UniTask> tasks = new List<UniTask>();
        for (int i = 0; i < 4; i++) if (hand[i] == null)
        {
            Card draw = GameManager.Instance.DrawCard();
            draw.player = this; hand[i] = draw;
            if (UIManager.Instance.curPlayer == this)
                tasks.Add(UIManager.Instance.UpdateUIRenderer(i, draw));
        }
        if (UIManager.Instance.curPlayer == this)
            await UniTask.WhenAll(tasks);
    }

    public async UniTask RefreshCard()
    {
        if (CommandCount <= 0) return;
        --CommandCount;
        for (int i = 0; i < 4; i++)
        {
            GameManager.Instance.DiscardCard(hand[i]);
            hand[i] = null;
        }
        await UpdateHandCard();
    }
    
    public async UniTask InitUI()
    {
        List<UniTask> tasks = new List<UniTask>();
        UIManager.Instance.SetCurPlayer(this);
        for (int i = 0; i < 4; i++)
            tasks.Add(UIManager.Instance.UpdateUIRenderer(i, hand[i]));
        await UniTask.WhenAll(tasks);
    }

    public bool hasWuXie(){
        for(int i=0;i<4;i++)
            if(hand[i] is WuXie)return true;
        return false;
    }
    public async UniTask<bool> useWuXie(Skill skill, Player usr, bool status)
    {
        UIManager.Instance.WuXieTitle.text = $"玩家{usr.id}正在使用【{skill.cardName}】，即将{(status ? "生效" : "失效")}，是否使用无懈可击？";
        UIManager.Instance.SwitchToWuXieUI();
        skill.ShowTarget();

        UniTaskCompletionSource<bool> UseTcs = new UniTaskCompletionSource<bool>();
        Action onUse = () => UseTcs.TrySetResult(true);
        Action onNotUse = () => UseTcs.TrySetResult(false);
        try
        {
            UIManager.Instance.UseWuXiePress += onUse;
            UIManager.Instance.NotUseWuXiePress += onNotUse;

            var tasks = new[] { UseTcs.Task, UniTask.Delay(3000) };
            int win = await UniTask.WhenAny(tasks);
            if (win == 1) return false;
            else if (await UseTcs.Task)
            {
                for (int i = 0; i < 4; i++)
                    if (hand[i] is WuXie) {
                        GameManager.Instance.DiscardCard(hand[i]);
                        hand[i] = null;
                        return true; 
                    }
            }
            else return false;
            return false;
        }
        finally
        {
            skill.RemoveShowTarget();
            UIManager.Instance.UseWuXiePress -= onUse;
            UIManager.Instance.NotUseWuXiePress -= onNotUse;
            UIManager.Instance.SwitchToNormalUI();
        }
    }
}