using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class CardRenderer : MonoBehaviour
{
    public Card data;
    public abstract UniTask InitSprite();
}

public interface IPieceRenderer
{
    public UniTask UpdatePosition();
    public UniTask UpdateRotation();
    public void UpdateData();
}