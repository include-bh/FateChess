using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Card
{
    public string cardName;
    public string cardDescription;
    public CardStatus status;
    public Sprite sprite;

    public CardRenderer rend;
    public virtual CardRenderer renderer
    {
        get => rend;
        set => rend = value;
    }
    
    [NonSerialized]public Player player;

    public Card()
    {
        status = CardStatus.InPile;
        player = null;
        renderer = null;
    }
    public virtual void InitCard()
    {
        status = CardStatus.InHand;
        player = null;
        renderer = null;
    }

    public virtual async UniTask<bool> UseCard(Player usr)
    {
        return false;
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

