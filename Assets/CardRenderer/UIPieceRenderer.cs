using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIPieceRenderer : UIRenderer,IPieceRenderer
{
    public Image backgroundRenderer;
    public TextMeshProUGUI HPText, ATText, DFText, RAText, STText;

    public async UniTask UpdatePosition()
    {
        rect.anchoredPosition = new Vector2(UIManager.UIXpos[pos], 10);
    }

    public async UniTask UpdateRotation()
    {
    }


    public void UpdateData()
    {
        if (data is Piece piece)
        {
            HPText.text = piece.HP.ToString();
            DFText.text = piece.DF.ToString();
            if (piece.ST != 0) STText.text = piece.ST.ToString();
            else STText.text = "";
            if (piece.AT != 0)
            {
                ATText.text = piece.AT.ToString();
                RAText.text = piece.RA.ToString();
            }
            else
            {
                ATText.text = "";
                RAText.text = "";
            }
        }
    }

    public override async UniTask InitSprite()
    {
        rend.sprite = data.sprite;
        rect.anchoredPosition = new Vector2(UIManager.UIXpos[pos], 10);
        if (data is Piece piece)
        {
            if (GameManager.Instance.playerAtlas != null)
            {
                string bgName = $"Player{piece.player.id}";
                backgroundRenderer.sprite = GameManager.Instance.playerAtlas.GetSprite(bgName);
            }
        }
        UpdateData();
    }

}
