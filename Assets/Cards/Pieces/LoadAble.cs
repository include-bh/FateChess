using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadAble : Piece
{
    public int capacity;
    public List<Servant> onLoad;

    public override void Move()
    {
        base.Move();
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
            x.ForceMove();
        onLoad.Clear();
    }
}
