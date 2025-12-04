using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
public class BoardRenderer : CardRenderer, IPointerEnterHandler, IPointerClickHandler
{
    public SpriteRenderer rend;
    public override async UniTask InitSprite()
    {
        rend.sprite = data.sprite;
        await InitAnimation();
    }
    public virtual async UniTask InitSprite(int tx, int ty, int tr)
    {
        rend.sprite = data.sprite;
        InitPosition(tx, ty);
        InitRotation(tr);
        await InitAnimation();
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (data != null)
        {
            UIManager.Instance.curOnCard = data;
            UIManager.Instance.UpdateDescription();
        }
    }
    public virtual void OnPointerClick(PointerEventData eventData)
    {
    }

    private bool isFlying;
    private float flyDuration = 1f;
    private Vector2 start,target;
    private AnimationCurve flyEase = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(1f, 1f) { inTangent = 0f, outTangent = 0f }
    );
    public void InitPosition(int tx, int ty)
    {
        transform.position = GameManager.GetPosition(tx, ty);
    }
    public void InitRotation(int tr)
    {
        transform.rotation = Quaternion.Euler(0, 0, GameManager.GetRotation(tr));
    }
    public async UniTask FlyTo(int tx, int ty)
    {
        float elapsed = 0f;

        target = GameManager.GetPosition(tx, ty);
        start = transform.position;

        while (elapsed < flyDuration)
        {
            elapsed += Time.unscaledDeltaTime; // 使用 unscaled 时间
            float t = flyEase.Evaluate(elapsed / flyDuration);
            Vector2 p = Vector2.Lerp(start, target, t);
            transform.position = p;

            await UniTask.Yield();
        }

        transform.position = target;
    }
    public async UniTask RotateTo(int tr)
    {
        float elapsed = 0f;

        var target = Quaternion.Euler(0, 0, GameManager.GetRotation(tr));
        var start = transform.rotation;

        while (elapsed < flyDuration)
        {
            elapsed += Time.unscaledDeltaTime; // 使用 unscaled 时间
            float t = flyEase.Evaluate(elapsed / flyDuration);
            Quaternion p = Quaternion.Lerp(start, target, t);
            transform.rotation = p;

            await UniTask.Yield();
        }

        transform.rotation = target;
    }

    public async UniTask InitAnimation()
    {
        float elapsed = 0f;

        while (elapsed < flyDuration)
        {
            elapsed += Time.unscaledDeltaTime; // 使用 unscaled 时间
            float t = flyEase.Evaluate(elapsed / flyDuration);

            float alpha = Mathf.Lerp(0.5f, 1f, t);
            float scale = Mathf.Lerp(2f, 1f, t);

            transform.localScale = scale * Vector2.one;
            SetAlpha(alpha);

            await UniTask.Yield();
        }

        transform.localScale = Vector2.one;
        SetAlpha(1);
    }
    
    public async UniTask LeaveAnimation()
    {
        float elapsed = 0f, duration = 1f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = flyEase.Evaluate(elapsed / flyDuration);
            float alpha = Mathf.Lerp(1f, 0.5f, t);
            SetAlpha(alpha);
            await UniTask.Yield();
        }
        await UniTask.Yield();
        Destroy(gameObject);
    }
    void SetAlpha(float alpha) {
        Color c = rend.color;
        c.a = alpha; rend.color = c;
    }
}
