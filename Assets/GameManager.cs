using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // 游戏状态变量
    public static readonly int[] dx = { 1, 0, -1, -1, 0, 1, 0 };
    public static readonly int[] dy = { 0, 1, 1, 0, -1, -1, 0 };

    public static readonly Vector2 ex = new Vector2(1f, (float)System.Math.Sqrt(3f));
    public static readonly Vector2 ey = new Vector2(-1f,(float)System.Math.Sqrt(3f));
    public Vector2 GetPosition(int x, int y)
    { return ex * x + ey * y; }
    public float GetRotation(int facing)
    { return 60f * facing; }

    public List<Card> CardPile = new List<Card>();
    public List<Card> DiscardPile = new List<Card>();

    public List<Player> players = new List<Player>();
    public SpriteAtlas playerAtlas;

    public GameObject tilePrefab;

    public Dictionary<(int x, int y), Tile> tiles = new Dictionary<(int x, int y), Tile>();
    public SpriteAtlas terrainAtlas;

    public void FlushCard()
    {
        int n = DiscardPile.Count;
        CardPile.Clear();
        CardPile.AddRange(DiscardPile);
        while (n > 1)
        {
            int k = Random.Range(0, n);
            --n;
            Card p = CardPile[k];
            CardPile[k] = CardPile[n];
            CardPile[n] = p;
        }
        DiscardPile.Clear();
    }

    public Card DrawCard()
    {
        if (CardPile.Count == 0)
        {
            if (DiscardPile.Count == 0)
            {
                Debug.Log("摸牌堆与弃牌堆同时为空，摸牌失败");
                return null;
            }
            //抛出“洗牌”事件
            FlushCard();
        }
        Card tmp = CardPile[0];
        CardPile.RemoveAt(0);
        return tmp;
    }

    public void DiscardCard(Card card)
    {
        if (card == null) return;
        DiscardPile.Add(card);
    }

    public Tile GetTile(int x, int y)
    {
        var key = (x, y);
        if (tiles.ContainsKey(key))
            return tiles[key];
        return null;
    }

    public bool AddBlock(int x,int y)
    {
        for (int i = 0; i < 7; i++)
            if (tiles.ContainsKey((x + dx[i], y + dy[i]))) return false;
        for (int i = 0; i < 7; i++)
            AddTile(x + dx[i], y + dy[i], Terrain.Plain, i == 6);
        return true;
    }
    public bool AddTile(int x, int y, Terrain typ, bool isCenter)
    {
        var key = (x, y);
        if (tiles.ContainsKey(key)) return false;
        if (tilePrefab != null)
        {
            GameObject newTileGO = Instantiate(tilePrefab);
            newTileGO.name = $"Tile_{x}_{y}";
            Tile newTile = newTileGO.GetComponent<Tile>();
            newTile.xpos = x;newTile.ypos = y;newTile.type = typ;newTile.isCenter = isCenter;
            tiles[key] = newTile;
            newTileGO.SetActive(true);
        }
        return true;
    }

    public bool RemoveTile(int x, int y)
    {
        var key = (x, y);
        if (!tiles.ContainsKey(key)) return false;
        DiscardCard(tiles[key].onTile);
        Tile t = tiles[key];
        Destroy(tiles[key].gameObject,0.01f);
        tiles.Remove(key);
        return true;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}