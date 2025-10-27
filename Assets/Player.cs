using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    public int CommandCnt = 0;
    public List<Card> hand = new List<Card>();
    public int id, side;
    public Master master;
    public List<Piece> onBoardList;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public virtual void OnMyTurn()
    {
        UIManager ui = GetComponent<UIManager>();
        if (ui != null)
        {

        }
    }

    public virtual (int, int) SelectPosition(List<(int, int)> PosSet)
    {
        return PosSet[0];
    }
    
    public virtual int SelectDirection()
    {
        return 0;
    }
}
