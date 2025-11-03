using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Player
{
    public int CommandCount = 0;
    public List<Card> hand = new List<Card>();
    public int id, team;
    public Master master;
    public List<Piece> onBoardList;

    public Player(int t=0)
    {
        team = t;
    }
    public virtual void OnMyTurn()
    {
        
    }

    public virtual (int, int) SelectPosition(List<(int, int)> PosSet)
    {
        return PosSet[0];
    }

    public virtual int SelectDirection()
    {
        return 0;
    }
    public virtual Piece SelectTarget(List<(int, int)> PosSet)
    {
        return null;
    }
}
