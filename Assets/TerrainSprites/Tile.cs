using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IPointerClickHandler
{
    public int xpos=0, ypos=0;
    public Terrain type=Terrain.Plain;
    public bool isCenter = false;
    public Piece onTile = null;

    public SpriteRenderer terrainRend;
    public bool isEditable = true;
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isEditable) return;
        if (type == Terrain.Water) type = Terrain.Plain;
        else ++type;

        UpdateSprite();
    }
    public void UpdateSprite()
    {
        if (GameManager.Instance.terrainAtlas != null && terrainRend != null)
        {
            string bgName = type.ToString();
            terrainRend.sprite = GameManager.Instance.terrainAtlas.GetSprite(bgName);
        }
    }
    public Tile()
    {
        xpos = ypos = 0;
        type = Terrain.Plain;
        isCenter = false;
        onTile = null;
        isEditable = true;
    }
    public Tile(int x, int y, Terrain t, bool c)
    {
        this.xpos = x;
        this.ypos = y;
        this.type = t;
        this.isCenter = c;
        this.onTile = null;
        this.isEditable = true;
        
        transform.position = GameManager.GetPosition(xpos, ypos);
    }
}


public enum Terrain
{
    Plain,
    Hill,
    Water,
}