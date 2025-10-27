using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glider : Vehicle
{
    
    public void Move()
    {
        canGoTo.Clear();
        foreach (var x in GameManager.Instance.tiles.Keys)
            canGoTo.Add(x);
    }
}
