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
    public override async UniTask UseCard(Player usr)
    {

        HashSet<(int, int)> buf = new HashSet<(int, int)>();
        foreach (Piece p in usr.onBoardList)
            if (p is UnitBase)
                for (int i = 0; i < 7; i++)
                {
                    Tile t = GameManager.Instance.GetTile(p.xpos + dx[i], p.ypos + dy[i]);
                    if (t == null) continue;
                    if (t.onTile == null) buf.Add((t.xpos, t.ypos));
                }

        (xpos, ypos) = await usr.SelectPosition(buf.ToList());
        tile = GameManager.Instance.GetTile(xpos, ypos);
        tile.onTile = this;
    }
    public override async UniTask Move()
    {
        await base.Move();
        foreach(Servant x in onLoad)
        {
            x.xpos = this.xpos;
            x.ypos = this.ypos;
        }
    }
    public override void OnDeath()
    {
        base.OnDeath();
        foreach (Servant x in onLoad)
        {
            if (tile.onTile == null && (x.canSwim || tile.type != Terrain.Water))
            {
                x.xpos = this.xpos;
                x.ypos = this.ypos;
                x.UpdateOnBoardPosition();
            }
            else x.ForceMove();
        }
        onLoad.Clear();
    }
}
