using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class UIRenderer : CardRenderer, IPointerEnterHandler, IPointerClickHandler
{
    public Image rend;
    public int pos;
    public RectTransform rect;

    void Awake()
    {
        gameObject.SetActive(false);
    }

    public override async UniTask InitSprite()
    {
        rend.sprite = data.sprite;
        rect.anchoredPosition = new Vector2(UIManager.UIXpos[pos], 10);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (data != null)
        {
            UIManager.Instance.curOnCard = data;
            UIManager.Instance.UpdateDescription();
        }
    }
    public async void OnPointerClick(PointerEventData eventData)
    {
        if (data is WuXie) return;
        await GameManager.Instance.curPlayer.UseCard(pos);
    }
    
    private bool isFlying;
    private float flyDuration = 1f;
    private AnimationCurve flyEase = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(1f, 1f) { inTangent = 0f, outTangent = 0f }
    );
    public async UniTask FlyIn()
    {
        gameObject.SetActive(true);
        float elapsed = 0f;
        float startY = -150f;
        float targetX = UIManager.UIXpos[pos];
        float targetY = 10f;
        
        rect.anchoredPosition = new Vector2(targetX, startY);
        gameObject.SetActive(true);

        while (elapsed < flyDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = flyEase.Evaluate(elapsed / flyDuration);
            float y = Mathf.Lerp(startY, targetY, t);
            rect.anchoredPosition = new Vector2(targetX, y);
            await UniTask.Yield();
        }

        rect.anchoredPosition = new Vector2(targetX, targetY);
    }
}