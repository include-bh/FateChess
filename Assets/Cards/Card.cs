using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public Image UIRend;
    public bool isSelected = false;

    public string cardName;

    public Card()
    {
    }
    public Card(string cardname)
    {
        cardName = cardname;
    }
    
    public void Select()
    {
        //GameManager.Instance.SelectCard(this);
        isSelected = true;
        // 可以添加选中效果，比如高亮边框
        transform.localScale = Vector3.one * 1.1f;
    }
    
    public void Deselect()
    {
        //GameManager.Instance.DeselectCard(this);
        isSelected = false;
        transform.localScale = Vector3.one;
    }

    public void OnClick()
    {
        if (isSelected) isSelected = false;
        else isSelected = true;
    }
    
    public virtual void UseCard(Player usr)
    {
        
    }

    public virtual void Start()
    {
        
    }

    // Update is called once per frame
    public virtual void Update()
    {
        
    }
}
