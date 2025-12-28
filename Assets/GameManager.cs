using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

public delegate int DamageModifier(int dmg, Piece atk, Piece def);

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // 游戏状态变量
    public static readonly int[] dx = { 1, 0, -1, -1, 0, 1, 0 };
    public static readonly int[] dy = { 0, 1, 1, 0, -1, -1, 0 };

    public static readonly Vector2 ex = new Vector2(1f, (float)System.Math.Sqrt(3f));
    public static readonly Vector2 ey = new Vector2(-1f, (float)System.Math.Sqrt(3f));

    public static readonly Vector2[] SelectTargetPos =
    {
        new Vector2(0f,1f),
        new Vector2(-(float)System.Math.Sqrt(3f)*0.5f, 0.5f),
        new Vector2(-(float)System.Math.Sqrt(3f)*0.5f, -0.5f),
        new Vector2(0f, -1f),
        new Vector2((float)System.Math.Sqrt(3f)*0.5f, -0.5f),
        new Vector2((float)System.Math.Sqrt(3f)*0.5f, 0.5f),
    };
    public static Vector2 GetPosition(int x, int y)
    { return ex * x + ey * y; }
    public static float GetRotation(int facing)
    { return 60f * facing; }

    public static int HexDist(int x1, int y1, int x2, int y2)
    {
        int z1 = -x1 - y1;
        int z2 = -x2 - y2;
        return (Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2) + Mathf.Abs(z1 - z2)) / 2;
    }


    public List<Card> CardPile = new List<Card>();
    public List<Card> DiscardPile = new List<Card>();
    public SpriteAtlas pieceAtlas;
    public SpriteAtlas skillAtlas;
    public SpriteAtlas weaponAtlas;


    public List<Player> players = new List<Player>();
    public SpriteAtlas playerAtlas;
    public Player curPlayer;

    public GameObject tilePrefab;

    public Dictionary<(int x, int y), Tile> tiles = new Dictionary<(int x, int y), Tile>();
    public SpriteAtlas terrainAtlas;


    public GameObject SelectPositionTagPrefab;
    public GameObject SelectDirectionTagPrefab;
    public GameObject BoardCardPrefab;
    public GameObject BoardPiecePrefab;
    public GameObject AttackEffectPrefab;

    public AudioSource OnHitAudioPlayer;

    public Physics2DRaycaster raycaster;

    public static event Action OnFlushCard;
    public void FlushCard()
    {
        int n = DiscardPile.Count;
        CardPile.Clear();
        CardPile.AddRange(DiscardPile);
        while (n > 1)
        {
            int k = UnityEngine.Random.Range(0, n);
            --n;
            Card p = CardPile[k];
            CardPile[k] = CardPile[n];
            CardPile[n] = p;
        }
        DiscardPile.Clear();
        OnFlushCard?.Invoke();
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
            Debug.Log("洗牌");
            FlushCard();
        }
        Card tmp = CardPile[0];
        CardPile.RemoveAt(0);
        tmp.InitCard();
        return tmp;
    }
    public void DiscardCard(Card card)
    {
        if (card == null) return;
        card.status = CardStatus.InPile;
        DiscardPile.Add(card);
    }

    public Tile GetTile(int x, int y)
    {
        var key = (x, y);
        if (tiles.ContainsKey(key))
            return tiles[key];
        return null;
    }
    public Tile AddTile(int x, int y, Terrain typ, bool isCenter)
    {
        var key = (x, y);
        if (tiles.ContainsKey(key)) return null;
        GameObject newTileGO = Instantiate(tilePrefab);
        newTileGO.name = $"Tile_{x}_{y}";
        Tile newTile = newTileGO.GetComponent<Tile>();
        newTile.xpos = x; newTile.ypos = y; newTile.type = typ; newTile.isCenter = isCenter;
        newTile.onTile = null;
        tiles[key] = newTile;
        newTile.transform.position = GetPosition(x, y);
        newTile.UpdateSprite();
        newTileGO.SetActive(true);
        return newTile;
    }
    public bool RemoveTile(int x, int y)
    {
        var key = (x, y);
        if (!tiles.ContainsKey(key)) return false;
        DiscardCard(tiles[key].onTile);
        Tile t = tiles[key];
        Destroy(tiles[key].gameObject, 0.01f);
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
    private Terrain GetRandType()
    {
        int x = UnityEngine.Random.Range(0, 10);
        if (x <= 0) return Terrain.Water;
        if (x <= 2) return Terrain.Hill;
        return Terrain.Plain;
    }
    void Start()
    {
        //初始化随机种子
        int seed = System.DateTime.Now.GetHashCode() ^
            System.Guid.NewGuid().GetHashCode() ^
            (int)(Time.realtimeSinceStartup * 1000);
        UnityEngine.Random.InitState(seed);

        //初始化玩家
        foreach (var cfg in DataLoader.PendingData)
        {
            if (cfg.type == PlayerType.Human)
                players.Add(new Player(cfg.team));
            if (cfg.type == PlayerType.AIBalanced)
                players.Add(new AIPlayer(cfg.team, 1, 1));
            if (cfg.type == PlayerType.AIAttack)
                players.Add(new AIPlayer(cfg.team, 2, 1));
            if (cfg.type == PlayerType.AIDefence)
                players.Add(new AIPlayer(cfg.team, 1, 2));
        }
        for (int i = 0; i < players.Count; i++)
            players[i].id = i;

        //生成牌堆
        for (int i = 0; i < 6; i++)
        {
            DiscardPile.Add(new Saber());
            DiscardPile.Add(new Archer());
            DiscardPile.Add(new Lancer());
        }
        for (int i = 0; i < 4; i++)
        {
            DiscardPile.Add(new Caster());
            DiscardPile.Add(new Rider());
            DiscardPile.Add(new Assassin());
            DiscardPile.Add(new Berserker());
        }

        for (int i = 0; i < 2; i++)
        {
            DiscardPile.Add(new Truck());
            DiscardPile.Add(new Glider());
            DiscardPile.Add(new Golem());

            DiscardPile.Add(new TianJia());
            DiscardPile.Add(new XiuGai());
            DiscardPile.Add(new HuoQiu());
            DiscardPile.Add(new GunMu());
            DiscardPile.Add(new JuFeng());
            DiscardPile.Add(new JinGu());
            DiscardPile.Add(new TuXi());
            DiscardPile.Add(new WuZhong());
            DiscardPile.Add(new WuXie());
        }

        DiscardPile.Add(new XiuGai());
        DiscardPile.Add(new YiDong());
        DiscardPile.Add(new ZhuanYi());
        DiscardPile.Add(new CeFan());


        DiscardPile.Add(new ExCalibur());
        DiscardPile.Add(new Avalon());
        DiscardPile.Add(new UBW());
        DiscardPile.Add(new GaeBolg());
        DiscardPile.Add(new RuleBreaker());
        DiscardPile.Add(new BloodFort());
        DiscardPile.Add(new GodHand());
        DiscardPile.Add(new Zabaniya());
        DiscardPile.Add(new TrailBat());
        DiscardPile.Add(new PyroLance());
        DiscardPile.Add(new OverbreakHat());
        DiscardPile.Add(new MemePen());
        DiscardPile.Add(new CludeSpear());
        DiscardPile.Add(new SunsetBow());

        FlushCard();

        //生成棋盘
        for (int i = 0; i < 7; i++)
        {
            AddTile(0 + dx[i], 0 + dy[i], GetRandType(), i == 6);
            AddTile(2 + dx[i], 1 + dy[i], GetRandType(), i == 6);
            AddTile(-1 + dx[i], 3 + dy[i], GetRandType(), i == 6);
            AddTile(-3 + dx[i], 2 + dy[i], GetRandType(), i == 6);
            AddTile(-2 + dx[i], -1 + dy[i], GetRandType(), i == 6);
            AddTile(1 + dx[i], -3 + dy[i], GetRandType(), i == 6);
            AddTile(3 + dx[i], -2 + dy[i], GetRandType(), i == 6);
        }
        foreach (var x in tiles.Values)
            x.isEditable = false;

        raycaster = Camera.main.GetComponent<Physics2DRaycaster>();
        raycaster.eventMask = LayerMask.GetMask("Piece");

        StartGame();
    }

    public async UniTask StartGame()
    {
        CardPile[0] = new Glider();
        CardPile[1] = new Rider();
        for (int i = 0; i < players.Count; i++)
        {
            Master mas = new Master();
            mas.InitCard();
            mas.player = players[i]; players[i].master = mas;
            players[i].onBoardList.Add(mas);

            List<(int, int)> buf = new List<(int, int)>();
            foreach (var ((x, y), t) in tiles)
            {
                if (t.onTile == null)
                    buf.Add((x, y));
            }

            (mas.xpos, mas.ypos) = await players[i].SelectPosition(buf);
            mas.facing = await players[i].SelectDirection(mas.xpos, mas.ypos);
            mas.status = CardStatus.OnBoard;

            GameObject go = Instantiate(BoardPiecePrefab);
            mas.renderer = go.GetComponent<BoardPieceRenderer>();
            mas.renderer.data = mas;
            mas.renderer.InitSprite();

            mas.UpdateOnBoardPosition();

            for (int j = 0; j < 4; j++)
            {
                Card draw = DrawCard();
                draw.player = players[i];
                players[i].hand.Add(draw);
            }
            await players[i].InitUI();
        }

        bool is_first = true;
        for (int i = 0; ; i = (i + 1) % players.Count)
        {
            if (players[i].dead) continue;
            await players[i].OnMyTurn(is_first ? 2 : 3);
            is_first = false;
        }
    }

    public void EndGame(int win)
    {
        UIManager.Instance.SwitchToDeathUI();
        raycaster.eventMask = 0;

        string buf = "获胜玩家：";
        foreach (Player x in players)
            if (x.team == win) buf += x.id.ToString() + "，";
        buf += "按下方按钮返回开始菜单";

        UIManager.Instance.DeathTitle.text = buf;
    }

    public async UniTask<bool> AskForWuXie(Skill skill, Player usr)
    {
        bool ok = true;
        int las = usr.id;
        HashSet<int> buf = new HashSet<int>();
        for (int i = (usr.id + 1) % players.Count; i != las; i = (i + 1) % players.Count)
            if (!players[i].dead && players[i].hasWuXie())
            {
                if (await players[i].useWuXie(skill, usr, ok))
                {
                    ok = !ok;
                    las = i;
                    buf.Add(i);
                }
            }
        foreach (int x in buf) players[x].UpdateHandCard();
        return ok;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }


    public void SetLose(Player player)
    {
        while (player.onBoardList.Count > 0) player.onBoardList[0].OnDeath();
        for (int i = 0; i < 4; i++)
        {
            DiscardCard(player.hand[i]);
            player.hand[i] = null;
        }
        player.dead = true;
        if (UIManager.Instance.curPlayer == player)
        {
            UIManager.Instance.DeathTitle.text = "你已被击败，按下方按钮返回开始菜单";
            UIManager.Instance.SwitchToDeathUI();
        }

        HashSet<int> teams = new HashSet<int>();
        foreach (Player p in players)
            if (p.dead == false) teams.Add(p.team);
        if (teams.Count == 1)
            foreach (int teamid in teams)
                EndGame(teamid);
    }
    
    public void Upgrade(Piece p1,Piece p2)
    {
        p2.renderer = p1.renderer;
        p2.xpos = p1.xpos;
        p2.ypos = p1.ypos;
        p2.facing = p1.facing;
        Player player = p1.player;

        p1.OnDeath();

        p2.UseCard(player);
    }
}