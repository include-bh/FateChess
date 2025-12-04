using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AttackEffect : MonoBehaviour
{
    public Transform rendTransform;
    public Vector2 start = new Vector2(-2, 0);
    public Vector2 end = new Vector2(0, 0);
    public float duration = 0.5f;


    public async UniTask PlayAnimation()
    {
        float distance = Vector2.Distance(start, end);
        float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;
        float scale = distance / 2;

        transform.position = end;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        transform.localScale = new Vector2(scale, 1f);

        float elapsed = 0f;
        float S = -2, T = 2;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float x = Mathf.Lerp(S, T, t);
            rendTransform.localPosition = new Vector2(x,0);
            await UniTask.Yield();
        }
        rendTransform.position = new Vector2(0, 0);
        Destroy(gameObject);
    }
}
