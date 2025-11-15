using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

[System.Serializable]
public class Player
{
    public int CommandCount = 0;
    public List<Card> hand = new List<Card>();
    public int id, team;
    public Master master=null;
    public List<Piece> onBoardList=new List<Piece>();


    public Player(int t=0)
    {
        team = t;
    }
    public virtual void OnMyTurn(int cmd)
    {
        GameManager.Instance.curPlayer = this;
        UIManager.Instance.curPlayer = this;
        CommandCount = cmd;
        foreach (Piece x in onBoardList) x.OnTurnBegin();
    }


    public List<SelectPositionTag> PositionTags=new List<SelectPositionTag>();
    public TaskCompletionSource<(int,int)> PositionTcs;
    public virtual async Task<(int, int)> SelectPosition(List<(int, int)> PosSet)
    {
        PositionTcs = new TaskCompletionSource<(int, int)>();
        foreach (var (x, y) in PosSet)
        {
            Vector2 pos = GameManager.Instance.GetPosition(x, y);
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
        
        return (resx,resy);
    }


    public List<SelectDirectionTag> DirectionTags=new List<SelectDirectionTag>();
    public TaskCompletionSource<int> DirectionTcs;
    public virtual async Task<int> SelectDirection(int xpos,int ypos)
    {
        DirectionTcs = new TaskCompletionSource<int>();
        Vector2 pos=GameManager.Instance.GetPosition(xpos,ypos);
        for (int i = 0; i < 6; i++)
        {
            GameObject go = GameObject.Instantiate(GameManager.Instance.SelectDirectionTagPrefab, pos, Quaternion.Euler(0, 0, 60f * i));
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

        return res;
    }
    public virtual async Task<Piece> SelectTarget(List<Piece> TargetSet)
    {
        return null;
    }


    public void UseCard(int id)
    {
        Card used = hand[id];
        Card draw = GameManager.Instance.DrawCard();
        hand[id] = draw;
        if (UIManager.Instance.curPlayer == this)
        {
            UIManager.Destroy(used.renderer.gameObject);
            UIManager.Instance.GenerateUIRenderer(id, draw);
        }
        used.UseCard(this);
    }
}
