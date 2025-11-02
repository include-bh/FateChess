using System.Collections;
using System.Collections.Generic;
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
                    if (newTile.onTile.belong != this.belong) continue;
                }
                if (newTile.type == Terrain.Water && canSwim == false) continue;
                int newdep = dep - 1;
                if (newTile.type == Terrain.Hill && canClimb == false) newdep = 0;
                dfsMove(newdep, nx, ny);
            }
        }
    }
    public override void Hit()
    {

    }
}
