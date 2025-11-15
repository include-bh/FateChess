using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Vehicle : LoadAble
{
    public override void dfsMove(int dep, int x, int y)
    {
        vis.Add((x, y));
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
                if (newTile.type == Terrain.Water && canSwim == false) continue;
                int newdep = dep - 1;
                if (newTile.type == Terrain.Hill && canClimb == false) newdep = 0;
                dfsMove(newdep, nx, ny);
            }
        }
    }
    public override UniTask Attack()
    {
        return UniTask.CompletedTask;
    }
}

public class Truck : Vehicle
{
}

public class Glider : Vehicle
{
    public override async UniTask Move()
    {
        
    }

    public override async UniTask Attack()
    {
        canGoTo.Clear();
        foreach (var x in GameManager.Instance.tiles.Keys)
            canGoTo.Add(x);
        var (nx, ny) = await player.SelectPosition(new List<(int, int)>(canGoTo));
        tile.onTile = null;
        foreach (Piece x in onLoad)
            (x.xpos, x.ypos) = (nx, ny);

        Piece e = GameManager.Instance.GetTile(nx, ny).onTile;
        if (e != null)
        {
            DealDamage(e, AT, true);
            if (e is LoadAble ve)
                foreach (Piece x in ve.onLoad)
                    DealDamage(x, AT, true);
        }
        OnDeath();
    }
}
