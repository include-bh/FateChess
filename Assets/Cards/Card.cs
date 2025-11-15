using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class Card
{
    public bool isSelected = false;
    public string cardName;
    public string cardDescription;
    public CardStatus status;
    public Sprite sprite;

    public CardRenderer renderer;

    public Card()
    {
        status = CardStatus.InPile;
        isSelected = false;
        renderer = null;
    }

    public void Select()
    {
        //GameManager.Instance.SelectCard(this);
        isSelected = true;
    }

    public void Deselect()
    {
        //GameManager.Instance.DeselectCard(this);
        isSelected = false;
    }

    public virtual async Task UseCard(Player usr)
    {

    }
    public virtual string GetDescription()
    {
        return cardDescription;
    }
}

[System.Serializable]
public enum CardStatus
{
    InPile,
    InHand,
    OnBoard,
}

