using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : Piece
{
    public List<Servant> onLoad;
    protected override void dfs(int dep, int x, int y)
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
                }
                if (newTile.type == Terrain.Water && canSwim == false) continue;
                int newdep = dep - 1;
                if (newTile.type == Terrain.Hill && canClimb == false) newdep = 0;
                dfs(newdep, nx, ny);
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
