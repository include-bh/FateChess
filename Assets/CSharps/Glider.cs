using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glider : Vehicle
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    public void Move()
    {
        canGoTo.Clear();
        foreach (var x in GameManager.Instance.tiles.Keys)
            canGoTo.Add(x);
        var (nx, ny, nf) = GameManager.Instance.TryGo(canGoTo);
        (xpos, ypos, facing) = (nx, ny, nf);
    }
}
