using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BoardPieceRenderer : BoardRenderer,IPieceRenderer
{
    public SpriteRenderer backgroundRenderer;
    public TextMeshPro HPText, ATText, DFText, RAText, STText;
    public override void UpdateSprite()
    {
        rend.sprite = data.sprite;
        if(data is Piece piece)
        {
            transform.position = GameManager.Instance.GetPosition(piece.xpos, piece.ypos);
            transform.rotation = Quaternion.Euler(0, 0, GameManager.Instance.GetRotation(piece.facing));

            if (GameManager.Instance.playerAtlas != null)
            {
                string bgName = $"Player{piece.player.id}";
                backgroundRenderer.sprite = GameManager.Instance.playerAtlas.GetSprite(bgName);
            }
            
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
}
