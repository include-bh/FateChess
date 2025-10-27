using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Servant : Piece
{
    // Start is called before the first frame update
    public void Attack(Piece e)
    {
        int x = AT;
        if (e.DF > x)
        {
            e.DF -= x;
        }
        else
        {
            x -= e.DF;
            e.DF = 0;
            EventManager.TriggerOnBreak(this,e);
            if (e.HP > x)
            {
                e.HP -= x;
            }
            else
            {
                e.HP = 0;
                EventManager.TriggerOnKill(this,e);
                enabled = false;
            }
        }
    }
}
