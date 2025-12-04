using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LoadAble : Piece
{
    public int capacity;
    public List<ICanOnLoad> onLoad;

    public LoadAble() : base()
    {
        onLoad = new List<ICanOnLoad>();
    }
    public override void InitCard()
    {
        base.InitCard();
        onLoad.Clear();
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
    
    public override async UniTask Move()
    {
        await base.Move();
        foreach(ICanOnLoad col in onLoad)if(col is Piece x)
        {
            x.xpos = this.xpos;
            x.ypos = this.ypos;
        }
    }
    public override void OnDeath()
    {
        base.OnDeath();
        foreach(ICanOnLoad col in onLoad)if(col is Piece x)
        {
            if (tile.onTile == null && (x.canSwim > 0 || tile.type != Terrain.Water))
            {
                x.xpos = this.xpos;
                x.ypos = this.ypos;
                x.UpdateOnBoardPosition();
            }
            else x.ForceMove();
        }
        onLoad.Clear();
    }
    public override string GetDescription()
    {
        string res = base.GetDescription();
        if (onLoad.Count > 0)
        {
            res += "\n内含：";
            foreach(ICanOnLoad col in onLoad)if(col is Piece x)
            {
                res += "\n" + x.cardName;
            }
        }
        return res;
    }
}
