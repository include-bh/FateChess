using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public int xpos, ypos;
    public Terrain type;
    public bool isCenter;
    public Piece onTile;

    public SpriteRenderer terrainRend;
    public bool isEditable;
    public void TryEdit()
    {
        if (type == Terrain.Water) type = Terrain.Plain;
        else ++type;
    }
    public void Update()
    {
        transform.position = GameManager.Instance.GetPosition(xpos, ypos);
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
    }
}


public enum Terrain
{
    Plain,
    Hill,
    Water,
}