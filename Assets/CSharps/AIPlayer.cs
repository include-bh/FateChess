using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPlayer : Player
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void OnMyTurn()
    {
        CommandCnt = 3;
        if (TryWinCondition()) return;

    }

    bool TryWinCondition()
    {
        return false;
    }
}
