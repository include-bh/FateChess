using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public Image cardImage;
    public Text cardNameText;
    public Text cardDescriptionText;
    
    private bool isSelected = false;
    public bool IsSelected => isSelected;

    public int id;
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
        isSelected = true;
        // 可以添加选中效果，比如高亮边框
        transform.localScale = Vector3.one * 1.1f;
    }
    
    public void Deselect()
    {
        isSelected = false;
        transform.localScale = Vector3.one;
    }
    
    public void OnClick()
    {
        //GameManager.Instance.ToggleCardSelection(this);
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
