using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class CardRenderer : MonoBehaviour
{
    public Card data;
    public abstract void UpdateSprite();
}

public interface IPieceRenderer
{
}