using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BoardRenderer : CardRenderer
{
    public SpriteRenderer rend;
    public override void UpdateSprite()
    {
        rend.sprite = data.sprite;
    }
}
