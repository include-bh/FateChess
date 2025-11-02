using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glider : Vehicle
{
    public override void Move()
    {
        
    }
    public override void Hit()
    {
        canHit.Clear();
        foreach (var x in GameManager.Instance.tiles.Keys)
            canHit.Add(x);
        var (nx, ny) = belong.SelectPosition(new List<(int, int)>(canHit));
        Piece e = GameManager.Instance.GetTile(nx, ny).onTile;
        xpos = nx;ypos = ny;
        if (e != null)
        {
            e.TakeDamage(AT);
            if (e is LoadAble ve)
                foreach (Piece x in ve.onLoad)
                    x.TakeDamage(AT);
        }
        OnDeath();
    }
}
