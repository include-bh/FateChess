using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageSpriteRenderer : MonoBehaviour
{
    public Image targetImage;
    public Sprite[] spriteArray;
    
    // 方法1：通过公共引用切换
    public void ChangeToSprite(Sprite newSprite)
    {
        if (targetImage != null && newSprite != null)
        {
            targetImage.sprite = newSprite;
        }
    }
    
    // 方法2：通过索引切换（使用数组）
    public void ChangeToSpriteByIndex(int index)
    {
        if (targetImage != null && 
            spriteArray != null && 
            index >= 0 && index < spriteArray.Length)
        {
            targetImage.sprite = spriteArray[index];
        }
    }
}