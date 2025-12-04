using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class BoardPieceRenderer : BoardRenderer, IPieceRenderer
{
    public SpriteRenderer backgroundRenderer;
    public TextMeshPro HPText, ATText, DFText, RAText, STText;

    public bool isSpan = false;

    public async UniTask UpdatePosition()
    {
        if (data is Piece piece)
            await FlyTo(piece.xpos, piece.ypos);
    }

    public async UniTask UpdateRotation()
    {
        if (data is Piece piece)
            await RotateTo(piece.facing);
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

    public override async void OnPointerClick(PointerEventData eventData)
    {
        if (data is Piece piece)
        {
            if (piece.player != GameManager.Instance.curPlayer) return;
            if (piece is LoadAble ve)
            {
                List<Piece> buf = new List<Piece> { ve };
                foreach (ICanOnLoad load in ve.onLoad)
                    if (load is Piece t && t.canAct) buf.Add(t);

                Piece tar = await GameManager.Instance.curPlayer.SelectTargetOnLoad(ve, buf);
                if(tar!=null)
                    tar.TakeAction();
            }
            else
                piece.TakeAction();
        }
    }

    public override async UniTask InitSprite()
    {
        rend.sprite = data.sprite;
        if (GameManager.Instance.playerAtlas != null)
        {
            string bgName = $"Player{data.player.id}";
            backgroundRenderer.sprite = GameManager.Instance.playerAtlas.GetSprite(bgName);
        }
        UpdateData();
        if (data is Piece piece)
        {
            InitPosition(piece.xpos, piece.ypos);
            InitRotation(piece.facing);
        }
        await InitAnimation();
    }

    public async UniTask TakeDamageAnimation()
    {
        float deg = (data is Piece p ? GameManager.GetRotation(p.facing) : 0);
        await UniTask.Delay(250);
        transform.localScale = 1.2f * Vector2.one;
        transform.rotation = Quaternion.Euler(0, 0, 15+deg);

        float elapsed = 0f, duration = 0.5f;
        float S1 = 1.2f, T1 = 1f;
        float S2 = 15f, T2 = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float x1 = Mathf.Lerp(S1, T1, t);
            float x2 = Mathf.Lerp(S2, T2, t);
            transform.localScale = x1 * Vector2.one;
            transform.rotation = Quaternion.Euler(0, 0, x2+deg);
            await UniTask.Yield();
        }
        transform.localScale = Vector2.one;
        transform.rotation = Quaternion.Euler(0, 0, deg);
    }
}
