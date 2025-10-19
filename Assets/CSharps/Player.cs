using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    public GameManager world;
    public int CommandCnt = 0;
    public Card[] cards = new Card[4];
    public int id, side;
    public Master master;
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
}
