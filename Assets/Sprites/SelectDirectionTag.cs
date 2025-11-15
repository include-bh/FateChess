using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectDirectionTag : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int xpos,ypos,facing;
    public SpriteRenderer rend;
    public Color defaultColor = new Color(1f, 0.5f, 0f);
    public Color hoverColor = new Color(1f, 0.75f, 0.5f);
    public Player player;
    

    void Awake()
    {
        rend.color=defaultColor;
    }
    public void OnPointerEnter(PointerEventData ed)
    {
        rend.color=hoverColor;
    }
    public void OnPointerExit(PointerEventData ed)
    {
        rend.color=defaultColor;
    }
    public void OnPointerClick(PointerEventData ed)
    {
        Debug.Log($"Click on SelectDirectionTag with facing {facing} 000000");
        player.DirectionTcs?.TrySetResult(facing);
        Debug.Log($"Click on SelectDirectionTag with facing {facing} 111111");
    }
}
