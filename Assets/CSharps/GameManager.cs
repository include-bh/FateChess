using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // 游戏状态变量
    public static readonly int[] dx = { 1, 0, -1, -1, 0, 1, 0 };
    public static readonly int[] dy = { 0, 1, 1, 0, -1, -1, 0 };


    public List<Card> CardPile = new List<Card>();
    public List<Card> DiscardPile = new List<Card>();
    public Transform handCardsParent;
    public Transform discardPileParent;

    public List<Player> players = new List<Player>();

    public Dictionary<(int x, int y), Tile> tiles = new Dictionary<(int x, int y), Tile>();

    private int getRandType()
    {
        int x = Random.Range(0, 9);
        if (x <= 6) return 0;
        if (x <= 8) return 1;
        return 2;
    }

    void FlushCard()
    {
        int n = DiscardPile.Count;
        while (n > 1)
        {
            int k = Random.Range(0, n);
            --n;
            Card p = DiscardPile[k];
            DiscardPile[k] = DiscardPile[n];
            DiscardPile[n] = p;
        }
        CardPile = DiscardPile;
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
    
    public Tile getTile(int x,int y)
    {
        var key = (x, y);
        if (tiles.ContainsKey(key))
            return tiles[key];
        return null;
    }
    public bool AddTile(int x,int y,int typ,bool isCenter)
    {
        var key = (x, y);
        if (tiles.ContainsKey(key)) return false;
        Tile newTile = new Tile(x, y, typ, isCenter);
        tiles[key] = newTile;
        return true;
    }

    public bool RemoveTile(int x,int y)
    {
        var key = (x, y);
        if (!tiles.ContainsKey(key)) return false;
        if(tiles[key].onTile != null)
        {
            Piece p = tiles[key].onTile;
            if (p is Hero) return false;
            if (p is UnitBase) return false;
        }
        DiscardCard(tiles[key].onTile);
        tiles.Remove(key);
        return true;
    }

    private void Awake()
    {
        // 确保只有一个实例
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 跨场景保持
        }
        else
        {
            Destroy(gameObject); // 销毁重复实例
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        //初始化随机数
        int seed = System.DateTime.Now.GetHashCode() ^ 
                    System.Guid.NewGuid().GetHashCode() ^ 
                    (int)(Time.realtimeSinceStartup * 1000);
        Random.InitState(seed);

        //生成牌堆
        /*for (int i = 0; i < 6; i++)
        {
            DiscardPile.Add(new Saber());
            DiscardPile.Add(new Archer());
            DiscardPile.Add(new Lancer());
        }
        for (int i = 0; i < 4; i++)
        {
            DiscardPile.Add(new Rider());
            DiscardPile.Add(new Caster());
            DiscardPile.Add(new Assassin());
            DiscardPile.Add(new Berserker());
        }
        for (int i = 0; i < 2; i++)
        {
            DiscardPile.Add(new Golem());
            DiscardPile.Add(new Truck());
            DiscardPile.Add(new Glider());
        }
        for (int i = 0; i < 2; i++)
        {
            DiscardPile.Add(new Skill("禁锢"));
            DiscardPile.Add(new Skill("奇兵突袭"));
            DiscardPile.Add(new Skill("无中生有"));
            DiscardPile.Add(new Skill("无懈可击"));
            DiscardPile.Add(new Skill("火球"));
            DiscardPile.Add(new Skill("滚木"));
            DiscardPile.Add(new Skill("飓风"));
            DiscardPile.Add(new Skill("添加棋盘块"));
            DiscardPile.Add(new Skill("地形修改"));
        }
        DiscardPile.Add(new Skill("地形修改"));
        DiscardPile.Add(new Skill("移动棋盘块"));
        DiscardPile.Add(new Skill("转移阵地"));

        DiscardPile.Add(new Weapon("誓约胜利之剑"));
        DiscardPile.Add(new Weapon("遥远的理想乡"));
        DiscardPile.Add(new Weapon("穿刺死棘之枪"));
        DiscardPile.Add(new Weapon("无限剑制"));
        DiscardPile.Add(new Weapon("鲜血神殿"));
        DiscardPile.Add(new Weapon("破除万法之符"));
        DiscardPile.Add(new Weapon("诅咒之手"));
        DiscardPile.Add(new Weapon("十二试炼"));
        DiscardPile.Add(new Weapon("开拓者的球棒"));
        DiscardPile.Add(new Weapon("筑城者的骑枪"));
        DiscardPile.Add(new Weapon("钟表匠的礼帽"));
        DiscardPile.Add(new Weapon("著者的羽毛笔"));
        DiscardPile.Add(new Weapon("溯时之枪"));
        DiscardPile.Add(new Weapon("裂空之箭"));*/
/*
        Card[] _cards = FindObjectsOfType<Card>();
        DiscardPile = new List<GameObject>();
        foreach (var x in _cards)
            DiscardPile.Add(x.gameObject);*/
        FlushCard();

        //生成棋盘
        for(int i = 0; i < 7; i++)
        {
            AddTile(0 + dx[i], 0 + dy[i], getRandType(), i==6);
            AddTile(2 + dx[i], 1 + dy[i], getRandType(), i==6);
            AddTile(-1+ dx[i], 3 + dy[i], getRandType(), i==6);
            AddTile(-3+ dx[i], 2 + dy[i], getRandType(), i==6);
            AddTile(-2+ dx[i], -1+ dy[i], getRandType(), i==6);
            AddTile(1 + dx[i], -3+ dy[i], getRandType(), i==6);
            AddTile(3 + dx[i], -2+ dy[i], getRandType(), i==6);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}

[System.Serializable]
public class Tile
{
    public int x, y;
    public int type;
    public bool isCenter;
    public Piece onTile;

    public Tile(int x, int y, int t, bool c)
    {
        this.x = x;
        this.y = y;
        this.type = t;
        this.isCenter = c;
        this.onTile = null;
    }
}