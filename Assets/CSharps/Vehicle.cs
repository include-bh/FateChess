using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : Piece
{
    private override void dfs_move(int dep, int x, int y)
    {
        Tile curTile = GameManager.Instance.getTile(x, y);
        if (curTile.onTile == null)
            canGoTo.Add((x, y));
        if (dep != 0)
        {
            for(int i = 0; i < 6; i++)
            {
                int nx = x + dx[i], ny = y + dy[i];
                Tile newTile = GameManager.Instance.getTile(nx, ny);
                if (newTile == null) continue;
                if (newTile.onTile != null)
                {
                    if (newTile.onTile.belong != this.belong) continue;
                    if (newTile.onTile is Vehicle ve)
                        if (ve.belong == this.belong && ve.onLoad.Count < 3) dfs(dep - 1, nx, ny);
                        else continue;
                    if (newTile.onTile is UnitBase ub)
                        if (ub.belong == this.belong && ub.onLoad.Count < 1) dfs(dep - 1, nx, ny);
                        else continue;
                }
                else
                {
                    if (newTile.type == 2 && canSwim == false) continue;
                    int newdep = dep - 1;
                    if (newTile.type == 1 && canClimb == false) newdep = 0;
                    dfs_move(newdep, nx, ny);
                }
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
