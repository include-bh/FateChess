using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Caster : Servant
{
    public override void Hit()
    {
        canHit.Clear();
        vis.Clear();
        dfsHit(RA, xpos, ypos);
        for(int i = 0; i < 3; i++)
        {
            Piece e = belong.SelectTarget(new List<(int,int)>(canHit));
            if (e != null) Attack(e);
            else break;
            if (e.canBanMagic) break;
            if (i != 2)
            {
                vis.Clear();
                dfsHit(RA, e.xpos, e.ypos);
            }
        }
    }
}
