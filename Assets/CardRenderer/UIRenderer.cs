using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class UIRenderer : CardRenderer, IPointerEnterHandler
{
    public Image rend;
    public int pos;
    public RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        gameObject.SetActive(false);
    }
    public override void UpdateSprite()
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
    public void FlyIn()
    {
        StartCoroutine(FlyInCoroutine());
    }

    private bool isFlying;
    private float flyDuration = 0.5f;
    private AnimationCurve flyEase = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(1f, 1f) { inTangent = 1f, outTangent = 0f }
    );
    private IEnumerator FlyInCoroutine()
    {
        if (isFlying) yield break;
        isFlying = true;

        float elapsed = 0f;
        float startY = -150f;
        float targetX = UIManager.UIXpos[pos];
        float targetY = 10;
        rect.anchoredPosition = new Vector2(targetX, startY);
        gameObject.SetActive(true);
        while (elapsed < flyDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = flyEase.Evaluate(elapsed / flyDuration);
            float y = Mathf.Lerp(startY, targetY, t);
            rect.anchoredPosition = new Vector2(targetX, y);
        }
        rect.anchoredPosition = new Vector2(targetX, targetY);
        isFlying = false;
    }
}